const { Client, LocalAuth } = require("whatsapp-web.js");
const path = require("path");
const fs = require("fs");

const { execSync } = require("child_process");
const { emitMessage } = require("../socket");
const qrcode = require("qrcode");
const { saveMedia } = require("../utils/mediaCache");

const MAX_QR_ATTEMPTS = 5;
const QR_EXPIRATION_MS = 2 * 60 * 1000;
const DEFAULT_DATA_PATH = "/data/.wwebjs_auth";
const PROFILE_LOCK_FILES = [
  "SingletonLock",
  "SingletonCookie",
  "SingletonSocket",
];

function getMessageType(msg) {
  if (!msg.hasMedia) return "chat";
  if (msg.type === "image") return "image";
  if (msg.type === "video") return "video";
  if (msg.type === "audio" || msg.type === "ptt") return "audio";
  if (msg.type === "sticker") return "sticker";
  return "document";
}

function isStatusBroadcast(msg) {
  return msg.from === "status@broadcast" || msg.to === "status@broadcast";
}

function clearProfileLocks(profilePath, userId) {
  try {
    if (!profilePath) return;
    fs.mkdirSync(profilePath, { recursive: true });
    PROFILE_LOCK_FILES.forEach((fileName) => {
      const lockPath = path.join(profilePath, fileName);
      if (fs.existsSync(lockPath)) {
        fs.rmSync(lockPath, { force: true });
        console.warn(`[${userId}] Arquivo de lock removido: ${lockPath}`);
      }
    });
  } catch (err) {
    console.warn(`[${userId}] Falha ao limpar locks do Chrome`, err);
  }
}

function terminateStaleChromeProcesses(profilePath, userId) {
  try {
    if (!profilePath) return;
    const psOutput = execSync("ps -eo pid,args", { encoding: "utf8" });
    const lines = psOutput.split("\n");
    const pids = lines.reduce((acc, line) => {
      const match = line.trim().match(/^(\d+)\s+(.*)$/);
      if (!match) return acc;
      const pid = Number(match[1]);
      const args = match[2];
      if (
        Number.isFinite(pid) &&
        args.includes(profilePath) &&
        (args.includes("chrome") || args.includes("chromium"))
      ) {
        acc.push(pid);
      }
      return acc;
    }, []);

    if (pids.length === 0) return;
    pids.forEach((pid) => {
      try {
        process.kill(pid, "SIGTERM");
      } catch (err) {
        console.warn(`[${userId}] Falha ao encerrar Chrome pid=${pid}`, err);
      }
    });
    execSync("sleep 0.5");
    pids.forEach((pid) => {
      try {
        process.kill(pid, 0);
        process.kill(pid, "SIGKILL");
      } catch (err) {
        // Processo já finalizado ou sem permissão.
      }
    });
    console.warn(
      `[${userId}] Processos do Chrome encerrados para liberar o perfil`,
    );
  } catch (err) {
    console.warn(`[${userId}] Falha ao verificar processos do Chrome`, err);
  }
}

async function getChatName(msg) {
  try {
    const chat = await msg.getChat();
    if (!chat) return null;
    if (chat.isGroup) return chat.name;
    return (
      chat.name ||
      chat?.contact?.pushname ||
      chat?.contact?.name ||
      msg.notifyName ||
      null
    );
  } catch (err) {
    console.warn("Falha ao obter nome do chat", err);
    return null;
  }
}

async function sendBackupMessage(payload) {
  const backupUrl = process.env.WHATSAPP_BACKUP_URL;

  if (!backupUrl) {
    console.warn("WHATSAPP_BACKUP_URL não definido. Backup ignorado.");
    return;
  }

  if (typeof fetch !== "function") {
    console.error("fetch não disponível no ambiente. Backup ignorado.");
    return;
  }

  try {
    const response = await fetch(backupUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const responseText = await response.text().catch(() => "");
      console.error(
        `Falha ao enviar backup (${response.status}). ${responseText}`,
      );
    }
  } catch (err) {
    console.error("Erro ao enviar backup para o backend", err);
  }
}

function createWhatsAppClient(userId, options = {}) {
  const { onInvalidated } = options;
  let qrCodeBase64 = null;
  let ready = false;
  let active = true;
  let invalidated = false;
  let qrAttempts = 0;
  let qrTimeoutId = null;
  const dataPath = process.env.WWEBJS_DATA_PATH || DEFAULT_DATA_PATH;
  const sessionPath = path.join(dataPath, `session-${userId}`);

  terminateStaleChromeProcesses(sessionPath, userId);
  clearProfileLocks(sessionPath, userId);

  const clearQrTimeout = () => {
    if (qrTimeoutId) {
      clearTimeout(qrTimeoutId);
      qrTimeoutId = null;
    }
  };

  const scheduleQrTimeout = () => {
    if (qrTimeoutId) return;
    qrTimeoutId = setTimeout(() => {
      if (ready || invalidated) return;
      console.warn(`[${userId}] QR Code expirado após ${QR_EXPIRATION_MS}ms`);
      qrCodeBase64 = null;
      invalidate("qr_timeout");
    }, QR_EXPIRATION_MS);
  };

  const invalidate = (reason) => {
    if (invalidated) return;
    invalidated = true;
    ready = false;
    active = false;
    clearQrTimeout();
    if (typeof onInvalidated === "function") {
      Promise.resolve(onInvalidated(reason)).catch((err) => {
        console.error(`[${userId}] Falha ao invalidar sessão`, err);
      });
    } else {
      client.destroy().catch((err) => {
        console.error(`[${userId}] Falha ao destruir cliente`, err);
      });
    }
  };

  const client = new Client({
    authStrategy: new LocalAuth({
      clientId: userId, // 🔥 chave do multi-usuário
      dataPath,
    }),
    puppeteer: {
      executablePath: process.env.PUPPETEER_EXECUTABLE_PATH,
      userDataDir: sessionPath,
      headless: true,
      timeout: 240000,
      protocolTimeout: 240000,
      args: [
        "--no-sandbox",
        "--disable-setuid-sandbox",
        "--disable-dev-shm-usage",
        "--disable-gpu",
      ],
    },
  });

  client.on("qr", async (qr) => {
    if (invalidated) return;
    qrAttempts += 1;
    if (qrAttempts > MAX_QR_ATTEMPTS) {
      console.warn(
        `[${userId}] Limite de tentativas de QR atingido (${MAX_QR_ATTEMPTS})`,
      );
      qrCodeBase64 = null;
      invalidate("qr_attempts_exceeded");
      return;
    }
    scheduleQrTimeout();
    qrCodeBase64 = await qrcode.toDataURL(qr);
    ready = false;
    console.log(`[${userId}] QR Code gerado`);
  });

  client.on("ready", () => {
    if (invalidated) return;
    ready = true;
    active = true;
    qrCodeBase64 = null;
    clearQrTimeout();
    console.log(`[${userId}] WhatsApp conectado`);
  });

  client.on("authenticated", () => console.log(`[${userId}] authenticated`));
  client.on("loading_screen", (percent, msg) =>
    console.log(`[${userId}] loading ${percent}% ${msg}`),
  );
  client.on("change_state", (state) =>
    console.log(`[${userId}] state: ${state}`),
  );

  client.on("auth_failure", (msg) => {
    ready = false;
    console.error(`[${userId}] Falha auth`, msg);
    invalidate("auth_failure");
  });

  client.on("disconnected", (reason) => {
    ready = false;
    console.warn(`[${userId}] WhatsApp desconectado`, reason);
    invalidate(reason);
  });

  client.on("message_create", async (msg) => {
    if (!msg.fromMe) return; // 🔑 chave da correção
    if (isStatusBroadcast(msg)) return;
    console.log("Mensagem enviada");

    let mediaUrl = null;
    if (msg.hasMedia) {
      // ⚠️ você NÃO deve mandar base64 pelo socket
      // Salve em disco / S3 / CDN
      mediaUrl = `/whatsapp/${userId}/messages/${msg.id._serialized}/media`;
    }

    const chatName = await getChatName(msg);

    emitMessage(userId, {
      chatId: msg.to,
      message: {
        id: msg.id._serialized,
        body: msg.body,
        fromMe: true,
        timestamp: msg.timestamp,
        type: getMessageType(msg),
        hasMedia: msg.hasMedia,
        mediaUrl,
      },
    });

    await sendBackupMessage({
      userId,
      chatId: msg.to,
      chatName,
      message: {
        id: msg.id._serialized,
        body: msg.body,
        fromMe: true,
        timestamp: msg.timestamp,
        type: getMessageType(msg),
        hasMedia: msg.hasMedia,
        mediaUrl,
      },
    });
  });

  client.on("message", async (msg) => {
    if (isStatusBroadcast(msg)) return;
    console.log("Nova mensagem recebida");

    let mediaUrl = null;

    if (msg.hasMedia) {
      // ⚠️ você NÃO deve mandar base64 pelo socket
      // Salve em disco / S3 / CDN
      mediaUrl = `/whatsapp/${userId}/messages/${msg.id._serialized}/media`;
    }

    const chatName = await getChatName(msg);

    emitMessage(userId, {
      chatId: msg.from,
      message: {
        id: msg.id._serialized,
        body: msg.body,
        fromMe: false,
        timestamp: msg.timestamp,
        type: getMessageType(msg),
        hasMedia: msg.hasMedia,
        mediaUrl,
      },
    });

    await sendBackupMessage({
      userId,
      chatId: msg.from,
      chatName,
      message: {
        id: msg.id._serialized,
        body: msg.body,
        fromMe: false,
        timestamp: msg.timestamp,
        type: getMessageType(msg),
        hasMedia: msg.hasMedia,
        mediaUrl,
      },
    });
  });

  client.initialize();

  return {
    client,
    getQr: () => qrCodeBase64,
    isReady: () => ready,
    isActive: () => active,
  };
}

module.exports = createWhatsAppClient;
