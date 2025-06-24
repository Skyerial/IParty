import { LayoutManager } from "../utils/layoutManager.js";

/**
 * JoystickController
 *
 * Renders a joystick input component within a layout and optionally adds extra buttons.
 * Utilizes LayoutManager to display the view and insert controls.
 */
export class JoystickController {
  /**
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * Initializes the controller by creating a LayoutManager,
   * rendering its view, adding a joystick component, and then adding any buttons.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addJoystick();
    await layout.addButtons();
  }
}
