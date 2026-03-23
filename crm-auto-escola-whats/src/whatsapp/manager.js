const path = require("path");
const fs = require("fs");
const createWhatsAppClient = require("./createClient");

const sessions = new Map();
const sessionLocks = new Map();

function getAuthPath(userId) {
  return path.resolve(
    process.env.WWEBJS_DATA_PATH || "./.wwebjs_auth",
    `session-${userId}`,
  );
}

function withSessionLock(userId, operation) {
  const previous = sessionLocks.get(userId) || Promise.resolve();
  const lockPromise = previous.catch(() => undefined).then(operation);
  const trackedPromise = lockPromise.finally(() => {
    if (sessionLocks.get(userId) === trackedPromise) {
      sessionLocks.delete(userId);
    }
  });

  sessionLocks.set(userId, trackedPromise);
  return lockPromise;
}

async function destroySessionClient(userId, session, { logout = true } = {}) {
  if (!session?.client) {
    return;
  }

  if (logout) {
    try {
      await session.client.logout();
    } catch (err) {
      console.error(`[${userId}] Falha ao fazer logout`, err);
    }
  }

  try {
    await session.client.destroy();
  } catch (err) {
    console.error(`[${userId}] Falha ao destruir cliente`, err);
  }
}

/**
 * Retorna sessão existente ou cria nova
 */
async function getSession(userId) {
  return withSessionLock(userId, async () => {
    const existing = sessions.get(userId);
    if (existing && typeof existing.isActive === "function" && existing.isActive()) {
      return existing;
    }

    if (existing) {
      sessions.delete(userId);
      await destroySessionClient(userId, existing, { logout: false });
      await fs.promises.rm(getAuthPath(userId), {
        recursive: true,
        force: true,
      });
    }

    const session = createWhatsAppClient(userId, {
      onInvalidated: async () => {
        await removeSession(userId, { logout: false });
      },
    });

    sessions.set(userId, session);
    return session;
  });
}

/**
 * Remove sessão (logout)
 */
async function removeSession(userId, options = {}) {
  const { logout = true } = options;

  return withSessionLock(userId, async () => {
    const session = sessions.get(userId);
    if (session) {
      sessions.delete(userId);
      await destroySessionClient(userId, session, { logout });
    }

    await fs.promises.rm(getAuthPath(userId), {
      recursive: true,
      force: true,
    });
  });
}

module.exports = {
  getSession,
  removeSession,
};
