import { HomeController } from "./controllers/homeController.js";
import { JoystickController } from "./controllers/joystickController.js";
import { NavController } from "./controllers/navController.js";

let root = document.querySelector(".view-container");
let navContainer = document.querySelector(".nav-container");
export let nav = new NavController(navContainer);

window.addEventListener("DOMContentLoaded", () => {
  const params = new URLSearchParams(window.location.search);
  const code = params.get("code");

  if (code && !isConnected()) {
    connectToServer(code);
    // Automatically load the controller when QR is used
    new JoystickController(root);
    nav.updateNavItem();
  } else {
    new HomeController(root);
  }
});

export let socket = null;

export function connectToServer(code) {
  if (socket) {
    return socket;
  }

  socket = new WebSocket(`ws://192.168.1.102:${code}`);

  socket.onopen = () => {
    console.log("connected");
  };

  socket.onmessage = (event) => {
    const data = JSON.parse(event.data);
    handle_controller_change(data);
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

export function connectionStatus() {
  if (!socket) return "no_socket";
  switch (socket.readyState) {
    case WebSocket.CONNECTING: return "connecting";
    case WebSocket.OPEN: return "connected";
    case WebSocket.CLOSING: return "closing";
    case WebSocket.CLOSED: return "closed";
    default: return "unknown";
  }
}

export function isConnecting() {
  return connectionStatus() === "connecting";
}

export function isConnected() {
  return connectionStatus() === "connected";
}

function handle_controller_change() {

}