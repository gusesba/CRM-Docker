const path = require("path");
const fs = require("fs");
const createWhatsAppClient = require("./createClient");

const sessions = new Map();
const sessionLocks = new Map();
const PROCESS_SCAN_ROOT = "/proc";
const SESSION_SHUTDOWN_TIMEOUT_MS = 10000;
const PROCESS_TERM_WAIT_MS = 1500;

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

function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function withTimeout(promise, ms, timeoutMessage) {
  let timeoutId = null;

  const timeoutPromise = new Promise((_, reject) => {
    timeoutId = setTimeout(() => {
      reject(new Error(timeoutMessage));
    }, ms);
  });

  return Promise.race([
    Promise.resolve(promise).finally(() => {
      if (timeoutId) {
        clearTimeout(timeoutId);
      }
    }),
    timeoutPromise,
  ]);
}

function isNumericProcessDir(entry) {
  return entry?.isDirectory?.() && /^\d+$/.test(entry.name);
}

function isChromeProcessCommand(commandLine) {
  return /(google-chrome|chrome|chromium)/i.test(commandLine);
}

function processExists(pid) {
  try {
    process.kill(pid, 0);
    return true;
  } catch (err) {
    return err?.code !== "ESRCH";
  }
}

async function listSessionBrowserProcesses(userId) {
  if (process.platform !== "linux") {
    return [];
  }

  const authPath = getAuthPath(userId);
  let entries = [];

  try {
    entries = await fs.promises.readdir(PROCESS_SCAN_ROOT, {
      withFileTypes: true,
    });
  } catch (err) {
    console.error(`[${userId}] Falha ao listar processos do sistema`, err);
    return [];
  }

  const matches = [];

  for (const entry of entries) {
    if (!isNumericProcessDir(entry)) {
      continue;
    }

    const pid = Number(entry.name);
    if (!Number.isInteger(pid) || pid === process.pid) {
      continue;
    }

    const cmdlinePath = path.join(PROCESS_SCAN_ROOT, entry.name, "cmdline");

    try {
      const rawCmdline = await fs.promises.readFile(cmdlinePath, "utf8");
      if (!rawCmdline) {
        continue;
      }

      const commandLine = rawCmdline.replace(/\0/g, " ").trim();
      if (!commandLine) {
        continue;
      }

      if (
        commandLine.includes(authPath) &&
        isChromeProcessCommand(commandLine)
      ) {
        matches.push({ pid, commandLine });
      }
    } catch (err) {
      if (!["ENOENT", "EACCES", "ESRCH"].includes(err?.code)) {
        console.error(
          `[${userId}] Falha ao ler cmdline do processo ${pid}`,
          err,
        );
      }
    }
  }

  return matches;
}

async function terminateSessionBrowserProcesses(userId) {
  const processes = await listSessionBrowserProcesses(userId);
  if (processes.length === 0) {
    return [];
  }

  console.warn(
    `[${userId}] Encerrando ${processes.length} processo(s) do navegador presos à sessão`,
  );

  for (const { pid } of processes) {
    try {
      process.kill(pid, "SIGTERM");
    } catch (err) {
      if (!["ESRCH", "EPERM"].includes(err?.code)) {
        console.error(`[${userId}] Falha ao enviar SIGTERM para ${pid}`, err);
      }
    }
  }

  await wait(PROCESS_TERM_WAIT_MS);

  const remaining = processes.filter(({ pid }) => processExists(pid));
  for (const { pid } of remaining) {
    try {
      process.kill(pid, "SIGKILL");
    } catch (err) {
      if (!["ESRCH", "EPERM"].includes(err?.code)) {
        console.error(`[${userId}] Falha ao enviar SIGKILL para ${pid}`, err);
      }
    }
  }

  return processes;
}

async function destroySessionClient(userId, session, { logout = true } = {}) {
  if (!session?.client) {
    await terminateSessionBrowserProcesses(userId);
    return;
  }

  if (logout) {
    try {
      await withTimeout(
        session.client.logout(),
        SESSION_SHUTDOWN_TIMEOUT_MS,
        `[${userId}] Timeout ao fazer logout do cliente`,
      );
    } catch (err) {
      console.error(`[${userId}] Falha ao fazer logout`, err);
    }
  }

  try {
    await withTimeout(
      session.client.destroy(),
      SESSION_SHUTDOWN_TIMEOUT_MS,
      `[${userId}] Timeout ao destruir cliente`,
    );
  } catch (err) {
    console.error(`[${userId}] Falha ao destruir cliente`, err);
  }

  await terminateSessionBrowserProcesses(userId);
}

/**
 * Retorna sessão existente ou cria nova
 */
async function getSession(userId) {
  return withSessionLock(userId, async () => {
    const existing = sessions.get(userId);
    if (
      existing &&
      typeof existing.isActive === "function" &&
      existing.isActive()
    ) {
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

    await terminateSessionBrowserProcesses(userId);

    const session = createWhatsAppClient(userId, {
      onInvalidated: async () => {
        await removeSession(userId, { logout: false });
      },
    });

    sessions.set(userId, session);
    return session;
  });
}

async function getExistingSession(userId) {
  return withSessionLock(userId, async () => {
    const existing = sessions.get(userId);
    if (
      existing &&
      typeof existing.isActive === "function" &&
      existing.isActive()
    ) {
      return existing;
    }

    if (existing) {
      sessions.delete(userId);
    }

    return null;
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

    if (!session) {
      await terminateSessionBrowserProcesses(userId);
    }

    await fs.promises.rm(getAuthPath(userId), {
      recursive: true,
      force: true,
    });
  });
}

module.exports = {
  getExistingSession,
  getSession,
  removeSession,
};
