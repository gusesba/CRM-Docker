require("dotenv").config();
const http = require("http");
const express = require("express");
const cors = require("cors");
const path = require("path");

const whatsappRoutes = require("./routes/whatsapp.routes");
const { initSocket } = require("./socket");
const { startMediaCleanup } = require("./utils/mediaCleanup");

const app = express();
app.use(cors());
app.use(express.json());
app.use("/whatsapp", whatsappRoutes);

startMediaCleanup({
  rootDir: path.join(__dirname, "media"),
  retentionDays: Number(process.env.WHATSAPP_MEDIA_RETENTION_DAYS || 7),
  intervalMinutes: Number(process.env.WHATSAPP_MEDIA_CLEAN_INTERVAL_MINUTES || 60),
});

const server = http.createServer(app);
initSocket(server);

server.listen(3001, () => {
  console.log("Servidor rodando na porta 3001");
});

app.use(
  "/media",
  express.static(path.join(__dirname, "media"))
);
