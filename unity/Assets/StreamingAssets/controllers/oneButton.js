import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Renders a view with a single customizable button using LayoutManager.
 * @details Initializes the layout and adds one default button without extra options.
 */
export class OneButton {
  /**
   * @brief Constructs a OneButton controller.
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * @brief Initializes the controller by creating a LayoutManager, rendering its view,
   *        and adding one default button to the layout.
   * @returns {Promise<void>} Resolves when the layout and button are rendered.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addButton();
  }
}
