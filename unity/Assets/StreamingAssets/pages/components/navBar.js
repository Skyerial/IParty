import { ConnectPage } from "../connectPage.js";
import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from "../../main.js";

/**
 * NavBar
 *
 * Displays the site navigation bar and handles logo clicks to navigate
 * to the connection page when not already connected.
 */
export class NavBar extends ViewRenderer {
  /**
   * @param {HTMLElement} container - The element where the navigation bar will be rendered.
   */
  constructor(container) {
    super("./views/navView.html", container);
  }

  /**
   * After the view loads, finds the logo element and sets up its click action.
   * If there is no active socket connection, clicking the logo opens the connect page.
   */
  bindEvents() {
    const viewContainer = document.querySelector(".view-container");

    document.querySelector(".site-logo").addEventListener("click", () => {
      if (!socketManager.isConnected()) {
        new ConnectPage(viewContainer);
      }
    });
  }
}