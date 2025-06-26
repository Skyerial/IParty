// relay.js

const fs = require('fs');
const https = require('https');
const express = require('express');
const WebSocket = require('ws');
const { v4: uuidv4 } = require('uuid');
const bodyParser = require('body-parser');

// SSL CERTIFICATES (Let’s Encrypt / DuckDNS)
const CERT_PATH = '/etc/letsencrypt/live/iparty.duckdns.org';
const sslOptions = {
  key: fs.readFileSync(`${CERT_PATH}/privkey.pem`),
  cert: fs.readFileSync(`${CERT_PATH}/fullchain.pem`)
};

// SERVER SETUP
const app = express();
app.use(bodyParser.raw({ type: '*/*', limit: '50mb' }));
const server = https.createServer(sslOptions, app);
const wss = new WebSocket.Server({ noServer: true });

const tunnels = {};

// WEBSOCKET UPGRADE HANDLER
server.on('upgrade', (request, socket, head) => {
  const url = new URL(request.url, `https://${request.headers.host}`);
  const parts = url.pathname.split('/');

  if (parts.length === 4 && parts[1] === 'unity') {
    wss.handleUpgrade(request, socket, head, (ws) => {
      ws.isUnity = true;
      wss.emit('connection', ws, request);
    });
  } else if (parts.length === 4 && parts[1] === 'host' && parts[3] === 'ws') {
    const hostId = parts[2];
    const info = tunnels[hostId];
    if (!info || !info.wsWS || info.wsWS.readyState !== WebSocket.OPEN) {
      console.warn(`[Relay] Browser tried WS on /host/${hostId}/ws but Unity WS tunnel not connected`);
      socket.write('HTTP/1.1 503 Service Unavailable\r\n\r\n');
      socket.destroy();
      return;
    }

    wss.handleUpgrade(request, socket, head, (clientSocket) => {
      const clientId = uuidv4();
      info.wsClients[clientId] = clientSocket;

      console.log(`[Relay] → External WS client connected (clientId=${clientId}, hostId=${hostId})`);

      clientSocket.on('message', (data) => {
        const payloadBase64 = Buffer.from(data).toString('base64');
        const wrap = { clientId, payloadBase64 };
        info.wsWS.send(JSON.stringify(wrap));
      });

      clientSocket.on('close', () => {
        delete info.wsClients[clientId];
        if (info.wsWS?.readyState === WebSocket.OPEN) {
          const wrap = { clientId, payloadBase64: null, event: 'disconnect' };
          info.wsWS.send(JSON.stringify(wrap));
        }
      });
    });
  } else {
    socket.destroy();
  }
});

// UNITY REGISTRATION HANDLING
wss.on('connection', (ws, request) => {
  const url = new URL(request.url, `https://${request.headers.host}`);
  const parts = url.pathname.split('/');
  const hostId = parts[2];
  const kind = parts[3];

  if (!tunnels[hostId]) {
    tunnels[hostId] = {
      httpWS: null,
      wsWS: null,
      pendingHttp: {},
      wsClients: {}
    };
  }

  const info = tunnels[hostId];

  if (kind === 'http') {
    info.httpWS = ws;
    console.log(`[Relay] Unity registered HTTP tunnel (hostId=${hostId})`);

    ws.on('message', (data) => {
      let msg;
      try {
        msg = JSON.parse(data.toString());
      } catch (err) {
        console.error('[Relay] Invalid HTTP tunnel message:', err);
        return;
      }

      const { requestId, status, bodyBase64, contentType } = msg;
      const pending = info.pendingHttp[requestId];
      if (!pending) return;

      clearTimeout(pending.timeout);
      delete info.pendingHttp[requestId];

      const buffer = Buffer.from(bodyBase64, 'base64');
      if (contentType) pending.res.setHeader('Content-Type', contentType);
      pending.res.status(status).send(buffer);
    });

    ws.on('close', () => {
      console.log(`[Relay] HTTP tunnel closed (hostId=${hostId})`);
      for (const rid in info.pendingHttp) {
        try {
          info.pendingHttp[rid].res.status(502).send('Unity HTTP tunnel closed');
        } catch {}
        clearTimeout(info.pendingHttp[rid].timeout);
        delete info.pendingHttp[rid];
      }
      info.httpWS = null;
    });
  }

  else if (kind === 'ws') {
    info.wsWS = ws;
    console.log(`[Relay] Unity registered WS tunnel (hostId=${hostId})`);

    ws.on('message', (data) => {
      let msg;
      try {
        msg = JSON.parse(data.toString());
      } catch (err) {
        console.error('[Relay] Invalid WS tunnel message:', err);
        return;
      }

      const { clientId, payloadBase64 } = msg;
      const clientSock = info.wsClients[clientId];
      if (!clientSock || clientSock.readyState !== WebSocket.OPEN) return;

      const buffer = Buffer.from(payloadBase64, 'base64');
      clientSock.send(buffer);
    });

    ws.on('close', () => {
      console.log(`[Relay] WS tunnel closed (hostId=${hostId})`);
      for (const cid in info.wsClients) {
        try {
          info.wsClients[cid].close(1011, 'Tunnel closed');
        } catch {}
      }
      info.wsWS = null;
      info.wsClients = {};
    });
  }
});

// EXTERNAL HTTP CLIENT HANDLER
app.all('/host/:hostId/http/*', (req, res) => {
  const hostId = req.params.hostId;
  const info = tunnels[hostId];
  if (!info || !info.httpWS || info.httpWS.readyState !== WebSocket.OPEN) {
    return res.status(502).send('Unity HTTP tunnel not connected');
  }

  const pathSuffix = req.params[0] || '';
  const requestId = uuidv4();
  const bodyBuf = req.body?.length ? req.body : Buffer.alloc(0);
  const bodyBase64 = bodyBuf.toString('base64');

  const payload = {
    requestId,
    method: req.method,
    url: '/' + pathSuffix,
    bodyBase64,
    contentType: req.get('Content-Type') || ''
  };

  const timeout = setTimeout(() => {
    if (info.pendingHttp[requestId]) {
      delete info.pendingHttp[requestId];
      try { res.status(504).send('Timeout waiting for Unity'); } catch {}
    }
  }, 10000);

  info.pendingHttp[requestId] = { res, timeout };
  info.httpWS.send(JSON.stringify(payload));
});

// START SERVER
const PORT = 5001;
server.listen(PORT, '0.0.0.0', () => {
  console.log(`Relay running on https://iparty.duckdns.org:${PORT}`);
});