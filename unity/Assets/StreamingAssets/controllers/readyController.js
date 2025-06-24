import { LayoutManager } from "../utils/layoutManager.js";

/**
 * ReadyController
 *
 * Manages the layout and insertion of a single “Ready” button into the UI.
 * Uses LayoutManager to render the view and add a customized button component.
 */
export class ReadyController {
  /**
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * Initializes the controller by creating a LayoutManager,
   * rendering its view, and adding a green “READY” button.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addButton({
      text: 'READY',
      className: 'green-ready'
    });
  }
}
