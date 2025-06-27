import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Initializes a directional pad (D-pad) and optional buttons within a given container.
 * @details Uses LayoutManager to append the D-pad control and default buttons to the UI.
 */
export class DpadController {
  /**
   * @brief Constructs a DpadController.
   * @param {HTMLElement} container - The DOM element to host the D-pad and buttons.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * @brief Initializes the layout by creating a LayoutManager and adding controls.
   * @returns {Promise<void>} Resolves when the D-pad and buttons have been added.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addDpad();
    await layout.addButtons();
  }
}
