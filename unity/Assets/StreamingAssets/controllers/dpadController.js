import { LayoutManager } from "../utils/layoutManager.js";

/**
 * DpadController initializes a directional pad (D-pad) and optional buttons within a given container.
 *
 * It uses LayoutManager to append the D-pad and control buttons to the UI.
 */
export class DpadController {
  /**
   * Create a DpadController.
   *
   * @param {HTMLElement} container - The DOM element to host the D-pad and buttons.
   */
  constructor(container) {
    /** @private @type {HTMLElement} */
    this.container = container;
  }

  /**
   * Initialize the layout by creating a LayoutManager and adding controls.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addDpad();
    await layout.addButtons();
  }
}