import { ConnectPage } from "../connectPage.js";
import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from "../../main.js";

/**
 * @brief Displays the site navigation bar and handles logo clicks to navigate
 *        to the connection page when not already connected.
 * @extends ViewRenderer
 */
export class NavBar extends ViewRenderer {
  /**
   * @brief Constructs a NavBar instance.
   * @param {HTMLElement} container - The element where the navigation bar will be rendered.
   */
  constructor(container) {
    super("./views/navView.html", container);
  }

  /**
   * @brief Called after the navigation HTML is loaded; sets up the logo click handler.
   * @details If there is no active socket connection, clicking the logo will open the ConnectPage.
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
