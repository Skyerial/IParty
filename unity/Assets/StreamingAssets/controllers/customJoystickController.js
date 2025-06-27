import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Initializes a custom joystick and button layout using LayoutManager.
 * @details Appends joystick and specific buttons ('C' and 'A') to the provided container.
 */
export class CustomJoystickController {
  /**
   * @brief Constructs a CustomJoystickController.
   * @param {HTMLElement} container - The DOM element to host the joystick and buttons.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * @brief Initializes the layout by creating a LayoutManager and adding controls.
   * @returns {Promise<void>} Resolves when joystick and buttons are added.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addJoystick();
    await layout.addButtons(["C", "A"]);
  }
}
