import { ConnectPage } from "./controllers/connectPage.js";
import { JoystickController } from "./controllers/joystickController.js";
import { NavBar } from "./controllers/navBar.js";

export let socket = null;

window.addEventListener("DOMContentLoaded", async () => {
  let root = document.querySelector(".view-container");
  let navContainer = document.querySelector(".nav-container");

  let nav = new NavBar(navContainer);
  await nav.init();

  const params = new URLSearchParams(window.location.search);
  const code = params.get("code");

  if (code && !isConnected()) {
    connectToServer(code);
    // Automatically load the controller when QR is used
    let js = new JoystickController(root);
    await js.init();
  } else {
    let cp = new ConnectPage(root);
    await cp.init();
  }
});

export function connectToServer(code) {
  if (socket) {
    return socket;
  }

  socket = new WebSocket(`ws://${location.hostname}:${code}`);

  socket.onopen = () => {
    console.log("connected");
  };

  socket.onmessage = (event) => {
    const data = JSON.parse(event.data);
    handle_data(data);
  };

  socket.onclose = () => {
    console.log("Disconnected from server");
    socket = null;
  };

  return socket;
}

export function disconnect() {
  if (socket && socket.readyState === WebSocket.OPEN) {
    socket.close();
  }
}

export function send(data) {
  if (socket.readyState === WebSocket.OPEN) {
    console.log("sending:", JSON.stringify(data));
    socket.send(JSON.stringify(data));
  }
}

export function isConnected() {
  return socket && socket.readyState === WebSocket.OPEN;
}

function handle_data(data) {}