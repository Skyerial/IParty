import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Initializes a gyroscopic control interface within a given container.
 * @details Uses LayoutManager to append a gyroscope input control to the UI.
 */
export class GyroController {
  /**
   * @brief Constructs a GyroController.
   * @param {HTMLElement} container - The DOM element to host the gyroscopic control.
   * @param {string} displayText - Text to display alongside the gyroscope control.
   */
  constructor(container, displayText) {
    this.container = container;
    this.displayText = displayText;
  }

  /**
   * @brief Initializes the layout by creating a LayoutManager and adding the gyro control.
   * @returns {Promise<void>} Resolves when layout and gyro control setup complete.
   */
  async init() {
    // Instantiate LayoutManager (horizontal layout)
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    // Add gyroscopic input control to the layout
    await layout.addGyro(this.displayText);
  }
}
