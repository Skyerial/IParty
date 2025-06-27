import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Manages the layout and insertion of a text input component into the UI.
 * @details Uses LayoutManager to render the view and add the text component with provided words.
 */
export class TextController {
  /**
   * @brief Constructs a TextController.
   * @param {HTMLElement} container - The element where the controller view will be rendered.
   * @param {string[]} words - Array of target words for the text component.
   */
  constructor(container, words) {
    this.container = container;
    this.words = words;
  }

  /**
   * @brief Initializes the controller by creating a LayoutManager, rendering its view,
   *        and adding a text input component with the given words.
   * @returns {Promise<void>} Resolves when layout and text component are rendered.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();
    await layout.addText(this.words);
  }
}
