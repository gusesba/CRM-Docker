const fs = require("fs");
const path = require("path");

async function removeOldFiles(rootDir, retentionMs, now = Date.now()) {
  let removed = 0;
  const entries = await fs.promises.readdir(rootDir, { withFileTypes: true });

  for (const entry of entries) {
    const absolutePath = path.join(rootDir, entry.name);
    if (entry.isDirectory()) {
      removed += await removeOldFiles(absolutePath, retentionMs, now);
      const remaining = await fs.promises.readdir(absolutePath);
      if (remaining.length === 0) {
        await fs.promises.rmdir(absolutePath);
      }
      continue;
    }

    if (!entry.isFile()) {
      continue;
    }

    const stats = await fs.promises.stat(absolutePath);
    if (stats.mtimeMs <= now - retentionMs) {
      await fs.promises.unlink(absolutePath);
      removed += 1;
    }
  }

  return removed;
}

function startMediaCleanup({
  rootDir,
  retentionDays = 7,
  intervalMinutes = 60,
  logger = console,
} = {}) {
  const retentionMs = retentionDays * 24 * 60 * 60 * 1000;
  if (!retentionMs || retentionMs <= 0) {
    logger.warn(
      "Limpeza automática de mídia desativada (WHATSAPP_MEDIA_RETENTION_DAYS <= 0)."
    );
    return null;
  }

  const runCleanup = async () => {
    try {
      const removed = await removeOldFiles(rootDir, retentionMs);
      if (removed > 0) {
        logger.info(`Limpeza de mídia: ${removed} arquivo(s) removido(s).`);
      }
    } catch (error) {
      logger.error("Erro na limpeza automática de mídia:", error);
    }
  };

  runCleanup();
  const intervalMs = intervalMinutes * 60 * 1000;
  return setInterval(runCleanup, intervalMs);
}

module.exports = {
  startMediaCleanup,
};
