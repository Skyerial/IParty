import { ConnectPage } from "./pages/connectPage.js";
import { SocketManager } from "./utils/socketManager.js";
import { NavBar } from "./pages/components/navBar.js";
import { LoginPage } from "./pages/loginPage.js";

export let socketManager = new SocketManager("iparty.duckdns.org");
let currController = null;

window.addEventListener("DOMContentLoaded", async () => {
  let root = document.querySelector(".view-container");
  let navContainer = document.querySelector(".nav-container");

  let nav = new NavBar(navContainer);
  await nav.init();

  const params = new URLSearchParams(window.location.search);
  const code = params.get("hostId");
  console.log(params);

  if (code && !socketManager.isConnected()) {
    socketManager.connect(code);
    let l = new LoginPage(root);
    await l.init();
    // Automatically load the controller when QR is used
    // let js = new JoystickController(root);
    // await js.init();
  } else {
    let cp = new ConnectPage(root);
    await cp.init();
  }
});

export function initializeController(Controller, container, param = null) {
  const c = param
    ? new Controller(container, param)
    : new Controller(container);

  c.init();
  setController(c);
}

function setController(controller) {
  currController = controller;
}

export function getController() {
  return currController;
}
