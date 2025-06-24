import { LayoutManager } from "../utils/layoutManager.js";

/**
 * TextController
 *
 * Manages the layout and insertion of a text input component into the UI.
 * Uses LayoutManager to render the view and add the text component.
 */
export class TextController {
  /**
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   */
  constructor(container) {
    this.container = container;
  }

  /**
   * Initializes the controller by creating a LayoutManager,
   * rendering its view, and adding a text input component.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();
    await layout.addText();
  }
}
