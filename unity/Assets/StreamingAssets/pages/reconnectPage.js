import { socketManager } from "../main.js";
import { ViewRenderer } from "../utils/viewRenderer.js";

/**
 * ReconnectPage
 *
 * Shows a reconnect interface where users re-enter a game code to rejoin.
 * Sends a reconnect request over the socket with the provided code.
 */
export class ReconnectPage extends ViewRenderer {
  /**
   * @param {HTMLElement} container - Element where the reconnect view will appear.
   */
  constructor(container) {
    super("./views/reconnectView.html", container);
  }

  /**
   * After the view loads, attaches a click handler to the reconnect button.
   * If the input is empty, shows an alert. Otherwise, sends a reconnect message.
   */
    reconnectEvent() {
        let inputfield = document.querySelector(".reconnect-input");
        document.querySelector(".reconnect").addEventListener("click", async () => {
            if (inputfield.value.trim() === "") {
                alert("Input field is empty!");
            }

            const data = { type: "reconnect", code: inputfield.value }
            socketManager.send(data)
        });
    }
}
