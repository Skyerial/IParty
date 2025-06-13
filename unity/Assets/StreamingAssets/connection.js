import { ConnectPage } from "./controllers/connectPage.js";
import { DpadController } from "./controllers/dpadController.js";
import { JoystickController } from "./controllers/joystickController.js";
import { NavBar } from "./controllers/navBar.js";
import { Login } from "./login/login.js";

export let socket = null;

window.addEventListener("DOMContentLoaded", async () => {
  let root = document.querySelector(".view-container");
  let navContainer = document.querySelector(".nav-container");

  let nav = new NavBar(navContainer);
  await nav.init();

  const params = new URLSearchParams(window.location.search);
  const code = params.get("hostId");

  if (code && !isConnected()) {
    connectToServer(code);
    let l = new Login(root)
    await l.init();
    // Automatically load the controller when QR is used
    // let js = new JoystickController(root);
    // await js.init();
  } else {
    let cp = new ConnectPage(root);
    await cp.init();
  }
});

export function connectToServer(code) {
  if (socket) {
    return socket;
  }

  const RELAY_HOST = '178.128.247.108';

  const isLocal = location.hostname === 'localhost' || location.hostname.startsWith('192.168.');
  const wsUrl = isLocal
    ? `ws://${location.hostname}:8181`
    : `ws://${RELAY_HOST}:5000/host/${code}/ws`

  socket = new WebSocket(wsUrl);

  socket.onopen = () => {
    console.log("connected");
  };

  socket.onmessage = (event) => {
    console.log(event.data);
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

function loadController(controller)
{
  let root = document.querySelector(".view-container");

  if (controller == "dpad-preset") {
    let js = new DpadController(root)
    js.init()
  } else if (controller == "joystick-preset") {
    let js = new JoystickController(root)
    js.init()
  }
}

function handle_data(data) {
  if (data.type == "controller") {
    loadController(data.controller)
  }
  console.log(data.type);
  console.log(data.controller);
}