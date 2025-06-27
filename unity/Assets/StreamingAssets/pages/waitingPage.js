import { ViewRenderer } from "../utils/viewRenderer.js";

/**
 * @brief Renders a waiting screen view until the game begins.
 * @extends ViewRenderer
 */
export class WaitingPage extends ViewRenderer {
  /**
   * @brief Constructs a WaitingPage.
   * @param {HTMLElement} container - The HTML container element where the view will be rendered.
   */
  constructor(container) {
    super("./views/waitingView.html", container);
  }
}
