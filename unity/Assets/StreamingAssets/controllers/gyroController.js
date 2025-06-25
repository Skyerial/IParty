import { LayoutManager } from "../utils/layoutManager.js";

/**
 * GyroController initializes a gyroscopic control interface within a given container.
 *
 * It uses LayoutManager to append a gyroscope input control to the UI.
 */
export class GyroController {
  /**
   * Create a GyroController.
   *
   * @param {HTMLElement} container - The DOM element to host the gyroscopic control.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * Initialize the layout by creating a LayoutManager and adding gyro control.
   */
  async init() {
    // Instantiate LayoutManager (horizontal layout)
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    // Add gyroscopic input control to the layout
    await layout.addGyro();
  }
}
