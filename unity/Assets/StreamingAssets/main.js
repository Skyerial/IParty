import { ConnectPage } from "./pages/connectPage.js";
import { SocketManager } from "./utils/socketManager.js";
import { NavBar } from "./pages/components/navBar.js";
import { LoginPage } from "./pages/loginPage.js";

/**
 * @brief Singleton instance of SocketManager for handling WebSocket communication.
 */
export let socketManager = new SocketManager("iparty.duckdns.org");

/**
 * @brief Reference to the currently active controller instance.
 */
let currController = null;

/**
 * @brief Entry point after DOM content is loaded; initializes navigation and appropriate page.
 */
window.addEventListener("DOMContentLoaded", async () => {
  /**
   * @brief Root container element for rendering views.
   */
  let root = document.querySelector(".view-container");
  /**
   * @brief Container element for rendering the navigation bar.
   */
  let navContainer = document.querySelector(".nav-container");

  // Initialize navigation bar
  let nav = new NavBar(navContainer);
  await nav.init();

  // Extract hostId parameter from URL
  const params = new URLSearchParams(window.location.search);
  const code = params.get("hostId");
  console.log(params);

  if (code && !socketManager.isConnected()) {
    // Connect to relay and show login page
    socketManager.connect(code);
    let l = new LoginPage(root);
    await l.init();
  } else {
    // No code or already connected: show Connect page
    let cp = new ConnectPage(root);
    await cp.init();
  }
});

/**
 * @brief Instantiates and initializes a controller, and sets it as the active controller.
 * @param {Class} Controller - The controller class to instantiate.
 * @param {HTMLElement} container - The DOM element to render the controller into.
 * @param {*} [param=null] - Optional parameter to pass to the controller’s constructor.
 */
export function initializeController(Controller, container, param = null) {
  const c = param
    ? new Controller(container, param)
    : new Controller(container);

  c.init();
  setController(c);
}

/**
 * @brief Sets the current active controller instance.
 * @param {*} controller - The controller instance to track.
 */
function setController(controller) {
  currController = controller;
}

/**
 * @brief Retrieves the currently active controller instance.
 * @returns {*} The active controller instance, or null if none is set.
 */
export function getController() {
  return currController;
}

/**
 * @brief Instantiates and initializes a new page, clearing the current controller if one exists.
 * @param {Class} Page - The page class to instantiate.
 * @param {HTMLElement} container - The container element to render the page into.
 */
export async function loadPage(Page, container) {
  new Page(container).init();

  if (currController) {
    setController(null);
  }
}
