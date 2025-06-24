import { LayoutManager } from "../utils/layoutManager.js";

/**
 * OneButton
 *
 * Renders a view with a single customizable button using LayoutManager.
 * This controller initializes the layout and adds one button without extra options.
 */
export class OneButton {
  /**
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * Initializes the controller by creating a LayoutManager,
   * rendering its view, and adding one default button.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addButton();
  }
}
