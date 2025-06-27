import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Manages the layout and insertion of a single “Ready” button into the UI.
 * @details Uses LayoutManager to render the view and add a customized button component.
 */
export class ReadyController {
  /**
   * @brief Constructs a ReadyController.
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * @brief Initializes the controller by creating a LayoutManager, rendering its view,
   *        and adding a green “READY” button.
   * @returns {Promise<void>} Resolves when the layout and button are rendered.
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
