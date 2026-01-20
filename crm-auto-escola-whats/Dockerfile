FROM node:20-bookworm

WORKDIR /app

# Instala Google Chrome (estável) dentro do container
RUN apt-get update && apt-get install -y wget gnupg ca-certificates \
  && wget -qO- https://dl.google.com/linux/linux_signing_key.pub | gpg --dearmor > /usr/share/keyrings/google-linux-signing-key.gpg \
  && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/google-linux-signing-key.gpg] http://dl.google.com/linux/chrome/deb/ stable main" \
     > /etc/apt/sources.list.d/google-chrome.list \
  && apt-get update && apt-get install -y google-chrome-stable \
  && rm -rf /var/lib/apt/lists/*

# Evita o puppeteer baixar browser próprio
ENV PUPPETEER_SKIP_DOWNLOAD=true
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/google-chrome

COPY package*.json ./
RUN npm ci

COPY . .

EXPOSE 3001
CMD ["npm", "start"]
