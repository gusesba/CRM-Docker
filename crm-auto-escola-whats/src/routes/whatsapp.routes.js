const express = require("express");
const { getSession, removeSession } = require("../whatsapp/manager");
const { saveMedia, getCachedMedia } = require("../utils/mediaCache");
const {
  getProfileImageUrlFromDisk,
  saveProfileImageFromUrl,
} = require("../utils/profileImageStorage");
const multer = require("multer");
const fs = require("fs");
const { MessageMedia } = require("whatsapp-web.js");

const BATCH_DELAY_MODE = "fixed";
const BATCH_FIXED_DELAY_MS = 1500;
const BATCH_RANDOM_DELAY_RANGE_MS = { min: 1000, max: 3000 };
const VALIDATE_TOKEN_URL = process.env.VALIDATE_TOKEN_URL || null;
async function validateToken(req, res, next) {
  const token = req.query.token ?? req.body?.token;

  if (
    req.method === "GET" &&
    req.path.match(/^\/[^/]+\/messages\/[^/]+\/media$/)
  ) {
    return next(); // ignora validação
  }

  if (!token) {
    return res.status(401).json({ error: "Token não informado" });
  }

  if (!VALIDATE_TOKEN_URL) {
    return res.status(500).json({ error: "URL de validação não configurada" });
  }

  try {
    const response = await fetch(VALIDATE_TOKEN_URL, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      return res.status(401).json({ error: "Token inválido" });
    }

    const payload = await response.json().catch(() => null);
    if (!payload?.valido) {
      return res.status(401).json({ error: "Token inválido" });
    }

    return next();
  } catch (error) {
    console.error("Erro ao validar token:", error);
    return res.status(500).json({ error: "Erro ao validar token" });
  }
}

function getMessageType(msg) {
  if (!msg.hasMedia) return "chat";
  if (msg.type === "image") return "image";
  if (msg.type === "video") return "video";
  if (msg.type === "audio" || msg.type === "ptt") return "audio";
  if (msg.type === "sticker") return "sticker";
  return "document";
}

function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function getRandomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function normalizeBase64(data) {
  if (typeof data !== "string") return null;
  if (data.startsWith("data:")) {
    const split = data.split(",");
    return split.length > 1 ? split[1] : null;
  }
  return data;
}

function normalizePhoneDigits(value) {
  if (typeof value !== "string" && typeof value !== "number") return null;
  const digits = String(value).replace(/\D/g, "");
  return digits.length > 0 ? digits : null;
}

function ensureCountryCode(digits) {
  if (!digits) return null;
  return digits.startsWith("55") ? digits : `55${digits}`;
}

function buildWhatsappCandidates(digits) {
  if (!digits) return [];
  const sanitized = ensureCountryCode(digits);
  if (!sanitized) return [];
  const local = sanitized.slice(2);
  const candidates = [];
  const addCandidate = (value) => {
    if (!value) return;
    if (!candidates.includes(value)) {
      candidates.push(value);
    }
  };

  let primary = sanitized;
  if (local.length === 10) {
    primary = `55${local.slice(0, 2)}9${local.slice(2)}`;
  }

  addCandidate(primary);

  if (local.length === 11) {
    addCandidate(`55${local.slice(0, 2)}${local.slice(3)}`);
  } else if (local.length === 10) {
    addCandidate(sanitized);
  }

  return candidates;
}

async function resolveWhatsappId(client, digits) {
  if (!client || !digits) return null;
  if (typeof client.getNumberId === "function") {
    const numberId = await client.getNumberId(digits);
    if (numberId?._serialized) {
      return numberId._serialized;
    }
  }
  if (typeof client.isRegisteredUser === "function") {
    const exists = await client.isRegisteredUser(digits);
    if (exists) {
      return `${digits}@c.us`;
    }
  }
  return null;
}

function applyTemplate(text, params = {}) {
  if (typeof text !== "string") return text;
  if (!params || typeof params !== "object") return text;
  return text.replace(/\$\{([^}]+)\}/g, (match, key) => {
    const value = params[key.trim()];
    if (value === undefined || value === null) return match;
    return String(value);
  });
}

async function buildChatResponse(chat, userId, session, lastMessageOverride = null) {
  const chatId = chat.id._serialized;
  let profilePicUrl = getProfileImageUrlFromDisk(userId, chatId);

  if (!profilePicUrl) {
    try {
      const remoteUrl = await withTimeout(
        session.client.getProfilePicUrl(chatId),
        3000,
        null
      );

      profilePicUrl = await saveProfileImageFromUrl(
        userId,
        chatId,
        remoteUrl
      );
    } catch (err) {
      console.error(`[${userId}] ⚠️ Avatar erro (${chatId}):`, err.message);
    }
  }

  let nmr = null;

  if (chatId.endsWith("@lid")) {
    const res = await session.client.getContactLidAndPhone([chatId]);

    if (res && res.length > 0) {
      const { pn } = res[0];
      if (pn) {
        nmr = pn.replace(/\D/g, "");
      }
    }
  }

  const lastMessageSource = lastMessageOverride || chat.lastMessage;

  return {
    id: chatId,
    name: chat.name || chat.id.user,
    isGroup: chat.isGroup,
    unreadCount: chat.unreadCount ?? 0,
    profilePicUrl,
    lastMessage: lastMessageSource
      ? {
          body: lastMessageSource.body,
          timestamp: lastMessageSource.timestamp,
        }
      : null,
    nmr,
    archived: chat.archived || false,
  };
}

const router = express.Router();
router.use(validateToken);

/**
 * LOGIN → QR CODE
 */
router.get("/:userId/login", (req, res) => {
  const { userId } = req.params;

  if(!userId || userId == undefined || userId == "undefined" || userId == null) return;

  const session = getSession(userId);

  if (session.isReady()) {
    return res.json({
      status: "connected",
      message: "WhatsApp já conectado",
    });
  }

  const qr = session.getQr();

  if (!qr) {
    return res.json({
      status: "waiting",
      message: "QR ainda não disponível",
    });
  }

  res.json({
    status: "qr",
    qrCode: qr,
  });
});

/**
 * LIMPAR SESSÃO (LOGOUT)
 */
router.delete("/:userId/session", async (req, res) => {
  const { userId } = req.params;

  if (!userId || userId === "undefined" || userId === null) {
    return res.status(400).json({ error: "Usuário inválido" });
  }

  try {
    await removeSession(userId);
    return res.json({ success: true, message: "Sessão removida" });
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: "Erro ao remover sessão" });
  }
});

function withTimeout(promise, ms, fallback = null) {
  return Promise.race([
    promise,
    new Promise((resolve) =>
      setTimeout(() => resolve(fallback), ms)
    ),
  ]);
}

/**
 * LISTAR CONVERSAS
 */
router.get("/:userId/conversations", async (req, res) => {
  const { userId } = req.params;
  const session = getSession(userId);

  if (!session || !session.isReady()) {
    console.log(`[${userId}] ❌ WhatsApp não conectado`);
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  console.log(`[${userId}] 🔄 Buscando conversas...`);

  try {
    const chats = await session.client.getChats();
    const total = chats.length;

    console.log(`[${userId}] 📦 ${total} chats encontrados`);

    const result = [];
    let processed = 0;

    const CONCURRENCY = 5;

    for (let i = 0; i < chats.length; i += CONCURRENCY) {
      const batch = chats.slice(i, i + CONCURRENCY);

      const batchResults = await Promise.all(
        batch.map(async (chat) => {
          const chatId = chat.id._serialized;

          let profilePicUrl = getProfileImageUrlFromDisk(userId, chatId);

          if (!profilePicUrl) {
            try {
              const remoteUrl = await withTimeout(
                session.client.getProfilePicUrl(chatId),
                3000, // ⏱️ 3s timeout
                null
              );

              profilePicUrl = await saveProfileImageFromUrl(
                userId,
                chatId,
                remoteUrl
              );
            } catch (err) {
              console.error(
                `[${userId}] ⚠️ Avatar erro (${chatId}):`,
                err.message
              );
            }
          }

          let nmr = null;

          if (chatId.endsWith("@lid")) {
            const res = await session.client.getContactLidAndPhone([chatId]);

            if (!res || res.length === 0) return null;

            const { pn } = res[0];
            if (!pn) return null;

            // remove + e caracteres não numéricos
            nmr = pn.replace(/\D/g, "");
          }

          processed++;
          return {
            id: chatId,
            name: chat.name || chat.id.user,
            isGroup: chat.isGroup,
            unreadCount: chat.unreadCount ?? 0,
            profilePicUrl,
            lastMessage: chat.lastMessage
              ? {
                  body: chat.lastMessage.body,
                  timestamp: chat.lastMessage.timestamp,
                }
              : null,
            nmr,
            archived: chat.archived || false,
          };
        })
      );

      result.push(...batchResults);

      const percent = Math.round((processed / total) * 100);
      console.log(
        `[${userId}] ⏳ Progresso: ${processed}/${total} (${percent}%)`
      );
    }

    console.log(`[${userId}] ✅ Conversas carregadas com sucesso`);
    res.json(result);
  } catch (err) {
    console.error(`[${userId}] ❌ Erro geral:`, err);
    res.status(500).json({ error: "Erro ao buscar conversas" });
  }
});


router.get("/:userId/messages/:chatId", async (req, res) => {
  const { userId, chatId } = req.params;
  const limit = Number(req.query.limit) || 50;

  const session = getSession(userId);

  if (!session || !session.isReady()) {
    return res.status(401).json({
      error: "WhatsApp não conectado",
    });
  }

  try {
    const chat = await session.client.getChatById(chatId);

    if (!chat) {
      return res.status(404).json({ error: "Chat não encontrado" });
    }

    const messages = await chat.fetchMessages({ limit });

    const result = messages.map((msg) => ({
      id: msg.id._serialized,
      body: msg.body,
      fromMe: msg.fromMe,
      timestamp: msg.timestamp,
      type: getMessageType(msg),
      hasMedia: msg.hasMedia,
      mediaUrl: msg.hasMedia
        ? `/whatsapp/${userId}/messages/${msg.id._serialized}/media`
        : null,
      author: msg.author || null,
    }));

    res.json(result);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Erro ao buscar mensagens" });
  }
});

router.get("/:userId/messages/:messageId/media", async (req, res) => {
  const { userId, messageId } = req.params;

  const session = getSession(userId);
  if (!session || !session.isReady()) {
    return res.status(401).end();
  }

  try {
    // ✅ PRIORIDADE TOTAL AO CACHE
    const cached = getCachedMedia(messageId);
    if (cached) {
      res.setHeader("Content-Type", cached.mimetype);
      res.setHeader("Content-Disposition", "inline");
      return res.sendFile(cached.absolutePath);
    }

    // 🔽 Só tenta baixar se NÃO estiver no cache
    const msg = await session.client.getMessageById(messageId);

    if (!msg || !msg.hasMedia) {
      return res.status(404).end();
    }

    const media = await msg.downloadMedia();
    const saved = saveMedia(media, messageId);

    res.setHeader("Content-Type", saved.mimetype);
    res.setHeader("Content-Disposition", "inline");
    return res.sendFile(saved.absolutePath);
  } catch (err) {
    console.error("Erro ao servir mídia:", err);
    return res.status(500).end();
  }
});

router.patch("/:userId/messages/:messageId", async (req, res) => {
  const { userId, messageId } = req.params;
  const { message } = req.body;

  const session = getSession(userId);
  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  if (typeof message !== "string" || message.trim() === "") {
    return res.status(400).json({ error: "Mensagem inválida" });
  }

  try {
    const msg = await session.client.getMessageById(messageId);

    if (!msg) {
      return res.status(404).json({ error: "Mensagem não encontrada" });
    }

    const edited = await msg.edit(message);

    if (!edited) {
      return res.status(400).json({
        success: false,
        error: "Mensagem não pode ser editada",
      });
    }

    return res.json({
      success: true,
      messageId: edited.id._serialized,
      body: edited.body,
    });
  } catch (err) {
    console.error(err);
    return res.status(500).json({
      success: false,
      error: "Erro ao editar mensagem",
    });
  }
});

router.delete("/:userId/messages/:messageId", async (req, res) => {
  const { userId, messageId } = req.params;
  const forEveryone =
    req.query.forEveryone === "true" || req.body?.forEveryone === true;

  const session = getSession(userId);
  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  try {
    const msg = await session.client.getMessageById(messageId);

    if (!msg) {
      return res.status(404).json({ error: "Mensagem não encontrada" });
    }

    // Não retorna boolean: se não deu erro, assume sucesso
    await msg.delete(forEveryone);

    return res.json({
      success: true,
      messageId: msg.id._serialized,
      forEveryone,
    });
  } catch (err) {
    console.error(err);
    return res.status(500).json({
      success: false,
      error: "Erro ao excluir mensagem",
    });
  }
});


router.post("/:userId/messages/batch", async (req, res) => {
  const { userId } = req.params;
  const {
    chatIds,
    items,
    paramsByChatId,
    intervalMs,
    bigIntervalMs,
    messagesUntilBigInterval,
  } = req.body;
  const session = getSession(userId);
  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }
  if (!Array.isArray(chatIds) || chatIds.length === 0) {
    return res.status(400).json({ error: "Lista de chats inválida" });
  }
  if (!Array.isArray(items) || items.length === 0) {
    return res.status(400).json({ error: "Lista de mensagens inválida" });
  }
  if (
    paramsByChatId !== undefined &&
    (typeof paramsByChatId !== "object" || Array.isArray(paramsByChatId))
  ) {
    return res.status(400).json({ error: "Parâmetros por chat inválidos" });
  }

  if (
    intervalMs !== undefined &&
    (typeof intervalMs !== "number" || Number.isNaN(intervalMs))
  ) {
    return res.status(400).json({ error: "Intervalo inválido" });
  }
  if (
    bigIntervalMs !== undefined &&
    (typeof bigIntervalMs !== "number" || Number.isNaN(bigIntervalMs))
  ) {
    return res.status(400).json({ error: "Intervalo grande inválido" });
  }
  if (
    messagesUntilBigInterval !== undefined &&
    (!Number.isInteger(messagesUntilBigInterval) ||
      messagesUntilBigInterval <= 0)
  ) {
    return res.status(400).json({ error: "Quantidade até intervalo grande inválida" });
  }

  const delayConfig =
    BATCH_DELAY_MODE === "fixed"
      ? {
          mode: "fixed",
          delayMs: BATCH_FIXED_DELAY_MS,
        }
      : {
          mode: "random",
          minMs: BATCH_RANDOM_DELAY_RANGE_MS.min,
          maxMs: BATCH_RANDOM_DELAY_RANGE_MS.max,
        };

  const shouldUseCustomDelay =
    intervalMs !== undefined ||
    bigIntervalMs !== undefined ||
    messagesUntilBigInterval !== undefined;
  
  for (const item of items) {
    if (item?.type === "text") {
      if (typeof item.message !== "string" || item.message.trim() === "") {
        return res.status(400).json({ error: "Mensagem de texto inválida" });
      }
    } else if (item?.type === "media") {
      const normalized = normalizeBase64(item.data);
      if (
        !normalized ||
        typeof item.mimetype !== "string" ||
        item.mimetype.trim() === ""
      ) {
        return res.status(400).json({ error: "Mídia inválida" });
      }
    } else {
      return res.status(400).json({ error: "Tipo de mensagem inválido" });
    }
  }

  const results = [];
  for (const chatId of chatIds) {
    const chatResult = { chatId, sent: [], errors: [] };
    try {
      const chat = await session.client.getChatById(chatId);
      if (!chat) {
        chatResult.errors.push("Chat não encontrado");
        results.push(chatResult);
        continue;
      }

      const randomizeDelay = (baseMs) => {
        if (typeof baseMs !== "number") {
          return 0;
        }
        const randomized = baseMs + getRandomInt(-3000, 3000);
        return Math.max(0, randomized);
      };

      const randomizeCount = (baseCount) => {
        if (!Number.isInteger(baseCount)) {
          return 0;
        }
        return Math.max(1, baseCount + getRandomInt(-3, 3));
      };

      const effectiveIntervalMs = shouldUseCustomDelay
        ? randomizeDelay(intervalMs ?? 0)
        : null;
      const effectiveBigIntervalMs = shouldUseCustomDelay
        ? randomizeDelay(bigIntervalMs ?? 0)
        : null;
      const effectiveMessagesUntilBigInterval = shouldUseCustomDelay
        ? randomizeCount(messagesUntilBigInterval ?? 0)
        : null;

      for (let index = 0; index < items.length; index += 1) {
        const item = items[index];
        const chatParams = paramsByChatId?.[chatId] ?? {};

        try {
          let sentMsg;

          if (item.type === "text") {
            const message = applyTemplate(item.message, chatParams);
            sentMsg = await chat.sendMessage(message, { sendSeen: false });
          } else {
            const data = normalizeBase64(item.data);
            const media = new MessageMedia(
              item.mimetype,
              data,
              item.filename || "media"
            );
            const caption =
              typeof item.caption === "string"
                ? applyTemplate(item.caption, chatParams)
                : item.caption;
            sentMsg = await chat.sendMessage(media, {
              caption,
              sendSeen: false
            });

            saveMedia(
              {
                data,
                mimetype: item.mimetype,
              },
              sentMsg.id._serialized
            );
          }

          chatResult.sent.push({
            index,
            type: item.type,
            messageId: sentMsg.id._serialized,
          });
        } catch (err) {
          console.error(err);
          chatResult.errors.push(
            `Erro ao enviar item ${index + 1}: ${err.message}`
          );
        }

        const hasNextMessage = index < items.length - 1;
        if (hasNextMessage) {
          let delayToUse;

          if (shouldUseCustomDelay) {
            const nextIndex = index + 1;
            const shouldUseBigInterval =
              effectiveMessagesUntilBigInterval > 0 &&
              nextIndex % effectiveMessagesUntilBigInterval === 0;
            delayToUse = shouldUseBigInterval
              ? effectiveBigIntervalMs
              : effectiveIntervalMs;
          } else {
            delayToUse =
              delayConfig.mode === "fixed"
                ? delayConfig.delayMs
                : getRandomInt(delayConfig.minMs, delayConfig.maxMs);
          }
          if (delayToUse > 0) {
            await wait(delayToUse);
          }
        }
      }
    } catch (err) {
      console.error(err);
      chatResult.errors.push(`Erro ao enviar para chat: ${err.message}`);
    }

    results.push(chatResult);
  }
  const success = results.every((result) => result.errors.length === 0);
  return res.json({ success, results });
});

router.post("/:userId/messages/number", async (req, res) => {
  const { userId } = req.params;
  const { number, message } = req.body;

  const session = getSession(userId);

  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  if (!message) {
    return res.status(400).json({ error: "Mensagem inválida" });
  }

  const digits = normalizePhoneDigits(number);
  if (!digits) {
    return res.status(400).json({ error: "Número inválido" });
  }

  const candidates = buildWhatsappCandidates(digits);
  if (candidates.length === 0) {
    return res.status(400).json({ error: "Número inválido" });
  }

  try {
    let resolvedId = null;
    let resolvedDigits = null;

    for (const candidate of candidates) {
      const chatId = await resolveWhatsappId(session.client, candidate);
      if (chatId) {
        resolvedId = chatId;
        resolvedDigits = candidate;
        break;
      }
    }

    if (!resolvedId) {
      return res.status(404).json({ error: "Número não existe no WhatsApp" });
    }

    const sentMsg = await session.client.sendMessage(resolvedId, message, {
      sendSeen: false
    });

    const chat = await session.client.getChatById(resolvedId);
    const chatResponse = chat
      ? await buildChatResponse(chat, userId, session, sentMsg)
      : null;

    return res.json({
      success: true,
      chat: chatResponse,
      normalizedNumber: resolvedDigits
    });
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: "Erro ao enviar mensagem" });
  }
});

router.post("/:userId/messages/:messageId/reply", async (req, res) => {
  const { userId, messageId } = req.params;
  const { message } = req.body;

  const session = getSession(userId);

  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  if (!message || typeof message !== "string") {
    return res.status(400).json({ error: "Mensagem inválida" });
  }

  try {
    const originalMessage = await session.client.getMessageById(messageId);

    if (!originalMessage) {
      return res.status(404).json({ error: "Mensagem não encontrada" });
    }

    const chat = await originalMessage.getChat();
    const sentMsg = await chat.sendMessage(message, {
      quotedMessageId: originalMessage.id._serialized,
      sendSeen: false
    });

    return res.json({
      success: true,
      messageId: sentMsg.id._serialized
    });
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: "Erro ao responder mensagem" });
  }
});

router.post("/:userId/arquivar", async (req, res) => {
  const { userId } = req.params;
  const { chatId, arquivar } = req.body;

  const session = getSession(userId);

  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  if (!chatId || typeof chatId !== "string") {
    return res.status(400).json({ error: "Chat inválido" });
  }

  if (typeof arquivar !== "boolean") {
    return res.status(400).json({ error: "Valor de arquivar inválido" });
  }

  try {
    const chat = await session.client.getChatById(chatId);

    if (!chat) {
      return res.status(404).json({ error: "Chat não encontrado" });
    }

    if (arquivar) {
      await chat.archive();
    } else {
      await chat.unarchive();
    }

    return res.json({ success: true, archived: arquivar });
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: "Erro ao atualizar conversa" });
  }
});

router.post("/:userId/addressbook/contact", async (req, res) => {
  const { userId } = req.params;
  const { phoneNumber, firstName, lastName } = req.body;

  const session = getSession(userId);

  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  try {
    const result = await session.client.saveOrEditAddressbookContact(
      phoneNumber,
      firstName,
      lastName,
      true);

    return res.json({ success: true, result, phoneNumber });
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: "Erro ao salvar contato" });
  }
});



router.post("/:userId/messages/:chatId", async (req, res) => {
  const { userId, chatId } = req.params;
  const { message } = req.body;

  const session = getSession(userId);

  if (!session || !session.isReady()) {
    return res.status(401).json({ error: "WhatsApp não conectado" });
  }

  if (!message) {
    return res.status(400).json({ error: "Mensagem inválida" });
  }

  try {
    const chat = await session.client.getChatById(chatId);
    await chat.sendMessage(message, {sendSeen: false});

    res.json({ success: true });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Erro ao enviar mensagem" });
  }
});

const upload = multer({ dest: "uploads/" });

router.post(
  "/:userId/messages/:chatId/media",
  upload.single("file"),
  async (req, res) => {
    const { userId, chatId } = req.params;
    const { caption } = req.body;
    const file = req.file;

    const session = getSession(userId);
    if (!session || !session.isReady()) {
      return res.status(401).json({ error: "WhatsApp não conectado" });
    }

    if (!file) {
      return res.status(400).json({ error: "Arquivo não enviado" });
    }

    try {
      const chat = await session.client.getChatById(chatId);

      const buffer = fs.readFileSync(file.path);
      const base64 = buffer.toString("base64");

      const media = new MessageMedia(
        file.mimetype,
        base64,
        file.originalname // 🔥 EXTREMAMENTE IMPORTANTE
      );

      // ✅ ENVIA E RECEBE A MENSAGEM REAL
      const sentMsg = await chat.sendMessage(media, { caption, sendSeen: false });

      // 🔥 AGORA SIM: salva no cache DEFINITIVO
      saveMedia(
        {
          data: fs.readFileSync(file.path, "base64"),
          mimetype: file.mimetype,
        },
        sentMsg.id._serialized
      );

      fs.unlinkSync(file.path); // limpa upload temporário

      res.json({
        success: true,
        messageId: sentMsg.id._serialized,
      });
    } catch (err) {
      console.error(err);
      res.status(500).json({ error: "Erro ao enviar mídia" });
    }
  }
);


module.exports = router;
