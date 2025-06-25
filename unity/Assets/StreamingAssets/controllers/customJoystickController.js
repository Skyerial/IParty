import { LayoutManager } from "../utils/layoutManager.js";

/**
 * CustomJoystickController initializes a custom joystick and button layout within a given container.
 * It uses LayoutManager to append joystick and button controls to the UI.
 */
export class CustomJoystickController {
  /**
   * Create a CustomJoystickController.
   *
   * @param {HTMLElement} container - The DOM element to host the joystick and buttons.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * Initialize the layout by creating a LayoutManager and adding controls.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addJoystick();
    await layout.addButtons(["C", "A"]);
  }
}