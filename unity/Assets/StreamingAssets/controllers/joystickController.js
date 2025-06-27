import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Renders a joystick input component within a layout and optionally adds extra buttons.
 * @details Utilizes LayoutManager to display the joystick view and insert controls.
 */
export class JoystickController {
  /**
   * @brief Constructs a JoystickController.
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * @brief Initializes the controller by creating a LayoutManager, rendering its view,
   *        adding the joystick component, and then adding any default buttons.
   * @returns {Promise<void>} Resolves when layout, joystick, and buttons have been added.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addJoystick();
    await layout.addButtons();
  }
}
