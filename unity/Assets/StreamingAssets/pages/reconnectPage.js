import { socketManager } from "../main.js";
import { ViewRenderer } from "../utils/viewRenderer.js";

/**
 * @brief Shows a reconnect interface where users re-enter a game code to rejoin.
 * @details Sends a reconnect request over the socket with the provided code.
 * @extends ViewRenderer
 */
export class ReconnectPage extends ViewRenderer {
  /**
   * @brief Constructs a ReconnectPage.
   * @param {HTMLElement} container - Element where the reconnect view will appear.
   */
  constructor(container) {
    super("./views/reconnectView.html", container);
  }

  /**
   * @brief Called after the view loads; attaches click handler to the reconnect button.
   * @details If the input is empty, shows an alert. Otherwise, sends a reconnect message via socketManager.
   */
  reconnectEvent() {
    let inputfield = document.querySelector(".reconnect-input");
    document.querySelector(".reconnect").addEventListener("click", async () => {
      if (inputfield.value.trim() === "") {
        alert("Input field is empty!");
      }

      const data = { type: "reconnect", code: inputfield.value };
      socketManager.send(data);
    });
  }
}
