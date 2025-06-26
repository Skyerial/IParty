import { ViewRenderer } from "../utils/viewRenderer.js";
import { socketManager } from "../main.js";
import { ReconnectPage } from "./reconnectPage.js";
import { LoginPage } from "./loginPage.js";
import { loadPage } from "../main.js";

/**
 * ConnectPage
 *
 * Presents a lobby interface where users enter or reconnect to a game code.
 * Handles both initial connection and reconnection flows.
 */
export class ConnectPage extends ViewRenderer {
  /**
   * @param {HTMLElement} container - Element where the connect view will appear.
   * @param {boolean} [created] - Indicates if a new lobby was just created.
   */
  constructor(container, created) {
    super("./views/connectView.html", container);
    this.created = created;
  }

  /**
   * After the view loads, attaches click handlers to the join and reconnect buttons.
   * - Join: opens a socket connection using the input value, then opens LoginPage.
   * - Reconnect: opens a socket connection using the input value, then opens ReconnectPage.
   */
  bindEvents() {
    const inputField = document.querySelector(".lobby-input");
    const joinBtn = document.querySelector(".lobby-button");
    const reconnectBtn = document.querySelector(".lobby-reconnect");

    joinBtn.addEventListener("click", async () => {
      if (!socketManager.isConnected()) {
        socketManager.connect(inputField.value);
      }
      await loadPage(LoginPage, this.container);
    });

    reconnectBtn.addEventListener("click", async () => {
      if (!socketManager.isConnected()) {
        socketManager.connect(inputField.value);
      }

      await loadPage(ReconnectPage, this.container);
    });
  }
}