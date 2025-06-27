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
  } else {
    let cp = new ConnectPage(root);
    await cp.init();
  }
});

/**
 * Instantiates and initializes a controller, and sets it as the current active controller.
 * @param {Class} Controller - The controller class to instantiate.
 * @param {HTMLElement} container - DOM element to render the controller into.
 * @param {*} [param=null] - Optional parameter to pass to the controller.
 */
export function initializeController(Controller, container, param = null) {
  const c = param
    ? new Controller(container, param)
    : new Controller(container);

  c.init();
  setController(c);
}

/**
 * Sets the current controller instance.
 * @param {*} controller - The controller instance to track.
 */
function setController(controller) {
  currController = controller;
}

/**
 * Gets the currently active controller instance.
 * @returns {*} - The active controller instance.
 */
export function getController() {
  return currController;
}

/**
 * Instantiates and initializes a new page, clearing the current controller if one exists.
 * @param {Class} Page - The page class to instantiate.
 * @param {HTMLElement} container - The container element to render the page into.
 */
export async function loadPage(Page, container) {
  new Page(container).init();

  if(currController) setController(null); 
}
