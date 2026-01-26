const fs = require("fs");
const path = require("path");

function msToMin(ms) {
  return Math.round(ms / 60000);
}

async function removeOldFiles(rootDir, retentionMs, now = Date.now()) {
  let removed = 0;
  let kept = 0;
  let visitedDirs = 0;

  let entries;
  try {
    entries = await fs.promises.readdir(rootDir, { withFileTypes: true });
  } catch (err) {
    // Pasta pode não existir ainda
    if (err.code === "ENOENT") {
      console.log(`[media-cleanup] Pasta não existe: ${rootDir}`);
      return { removed: 0, kept: 0, visitedDirs: 0 };
    }
    throw err;
  }

  visitedDirs += 1;
  console.log(`[media-cleanup] Verificando pasta: ${rootDir} (${entries.length} itens)`);

  for (const entry of entries) {
    const absolutePath = path.join(rootDir, entry.name);

    if (entry.isDirectory()) {
      const sub = await removeOldFiles(absolutePath, retentionMs, now);
      removed += sub.removed;
      kept += sub.kept;
      visitedDirs += sub.visitedDirs;

      // remove diretório vazio
      const remaining = await fs.promises.readdir(absolutePath).catch(() => []);
      if (remaining.length === 0) {
        await fs.promises.rmdir(absolutePath);
        console.log(`[media-cleanup] Pasta vazia removida: ${absolutePath}`);
      }
      continue;
    }

    if (!entry.isFile()) {
      console.log(`[media-cleanup] Ignorando (não é arquivo): ${absolutePath}`);
      continue;
    }

    let stats;
    try {
      stats = await fs.promises.stat(absolutePath);
    } catch (err) {
      console.log(`[media-cleanup] Não consegui ler stats de: ${absolutePath}`, err);
      continue;
    }

    const ageMs = now - stats.mtimeMs;
    const shouldRemove = stats.mtimeMs <= now - retentionMs;

    if (shouldRemove) {
      await fs.promises.unlink(absolutePath);
      removed += 1;
      console.log(
        `[media-cleanup] REMOVIDO: ${absolutePath} (idade ~${msToMin(ageMs)} min)`
      );
    } else {
      kept += 1;
      console.log(
        `[media-cleanup] Mantido: ${absolutePath} (idade ~${msToMin(ageMs)} min)`
      );
    }
  }

  return { removed, kept, visitedDirs };
}

function startMediaCleanup({
  rootDir,
  retentionDays = 7,
  intervalMinutes = 60
} = {}) {
  const retentionMs = retentionDays * 24 * 60 * 60 * 1000;

  if (!retentionMs || retentionMs <= 0) {
    console.log(
      "[media-cleanup] Limpeza automática de mídia desativada (WHATSAPP_MEDIA_RETENTION_DAYS <= 0)."
    );
    return null;
  }

  const intervalMs = intervalMinutes * 60 * 1000;

  console.log(
    `[media-cleanup] Ativado. rootDir=${rootDir} | retentionDays=${retentionDays} (~${msToMin(retentionMs)} min) | intervalMinutes=${intervalMinutes}`
  );

  const runCleanup = async () => {
    const startedAt = new Date().toISOString();
    console.log(`[media-cleanup] Iniciando execução: ${startedAt}`);

    try {
      const { removed, kept, visitedDirs } = await removeOldFiles(
        rootDir,
        retentionMs,
        Date.now()
      );

      console.log(
        `[media-cleanup] Fim da execução: removidos=${removed}, mantidos=${kept}, pastas_visitadas=${visitedDirs}`
      );
    } catch (error) {
      console.log("[media-cleanup] Erro na limpeza automática de mídia:", error);
    }
  };

  // roda já ao subir
  runCleanup();

  // agenda próximas rodadas
  const timer = setInterval(runCleanup, intervalMs);
  console.log(`[media-cleanup] Próxima execução a cada ${intervalMinutes} min.`);
  return timer;
}

module.exports = {
  startMediaCleanup,
};
