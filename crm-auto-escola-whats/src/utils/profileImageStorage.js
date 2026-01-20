const fs = require("fs");
const path = require("path");

const IMAGE_SIZE = 64;
const IMAGE_QUALITY = 40;

function getSafeName(value) {
  return String(value)
    .normalize("NFKD")
    .replace(/[^a-zA-Z0-9._-]/g, "_");
}

function getProfileImageInfo(userId, chatId) {
  const safeUserId = getSafeName(userId);
  const safeChatId = getSafeName(chatId);
  const dir = path.join(__dirname, "..", "media", "imagens", safeUserId);
  const filename = `${safeChatId}.jpg`;
  const absolutePath = path.join(dir, filename);
  const url = `/media/imagens/${safeUserId}/${encodeURIComponent(filename)}`;

  return { dir, absolutePath, url };
}

function getLegacyProfileImageInfo(userId, chatId) {
  const legacyUserId = encodeURIComponent(String(userId));
  const legacyChatId = encodeURIComponent(String(chatId));
  const dir = path.join(__dirname, "..", "media", "imagens", legacyUserId);
  const filename = `${legacyChatId}.jpg`;
  const absolutePath = path.join(dir, filename);
  const url = `/media/imagens/${encodeURIComponent(
    legacyUserId
  )}/${encodeURIComponent(filename)}`;

  return { absolutePath, url };
}

function getProfileImageUrlFromDisk(userId, chatId) {
  const { absolutePath, url } = getProfileImageInfo(userId, chatId);
  if (fs.existsSync(absolutePath)) {
    return url;
  }
  const legacy = getLegacyProfileImageInfo(userId, chatId);
  if (fs.existsSync(legacy.absolutePath)) {
    return legacy.url;
  }
  return null;
}

function withLowQualityParams(imageUrl) {
  try {
    const url = new URL(imageUrl);
    url.searchParams.set("size", IMAGE_SIZE);
    url.searchParams.set("quality", IMAGE_QUALITY);
    return url.toString();
  } catch (err) {
    return imageUrl;
  }
}

async function saveProfileImageFromUrl(userId, chatId, imageUrl) {
  if (!imageUrl) return null;
  const { dir, absolutePath, url } = getProfileImageInfo(userId, chatId);

  if (fs.existsSync(absolutePath)) {
    return url;
  }

  try {
    const response = await fetch(withLowQualityParams(imageUrl));
    if (!response.ok) {
      return null;
    }

    const arrayBuffer = await response.arrayBuffer();
    const buffer = Buffer.from(arrayBuffer);

    await fs.promises.mkdir(dir, { recursive: true });
    await fs.promises.writeFile(absolutePath, buffer);

    return url;
  } catch (err) {
    console.error("Erro ao salvar avatar em disco:", err);
    return null;
  }
}

module.exports = {
  getProfileImageUrlFromDisk,
  saveProfileImageFromUrl,
};
