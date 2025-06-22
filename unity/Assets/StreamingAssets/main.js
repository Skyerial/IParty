import { DpadController } from "./controllers/dpadController.js";
import { JoystickController } from "./controllers/joystickController.js";
import { Login } from "./login/login.js";
import { ConnectPage } from "./pages/connectPage.js";
import { SocketManager } from "./utils/socketManager.js";
import { NavBar } from "./pages/navBar.js";

export let socketManager = new SocketManager('iparty.duckdns.org');
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