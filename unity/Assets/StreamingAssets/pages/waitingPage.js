import { ViewRenderer } from "../utils/viewRenderer.js";

/**
 * Class representing a waiting page view.
 * Extends the ViewRenderer to load and render the waiting screen HTML.
 */
export class WaitingPage extends ViewRenderer {

  /**
   * Creates an instance of WaitingPage.
   * @param {HTMLElement} container - The HTML container element where the view will be rendered.
   */
  constructor(container) {
    super("./views/waitingView.html", container);
  }
}