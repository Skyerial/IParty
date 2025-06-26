import { socketManager } from "../../main.js";
import { ViewRenderer } from "../../utils/viewRenderer.js";


/**
 * TextComponent renders a text input UI and emits text updates through the socketManager.
 *
 * @extends ViewRenderer
 */
export class TextComponent extends ViewRenderer {
  /**
   * Create a TextComponent.
   *
   * @param {HTMLElement} container - The DOM element to render the text input into.
   * @param {boolean} [vertical=false] - If true, adjusts behavior for vertical layout (unused currently).
   */
  constructor(container, vertical = false, words) {
    super("./views/components/textComponentView.html", container, "textInput");
    this.vertical = vertical;
    this.origin = { x: 0, y: 0 };
    this.words = words;
    this.currentIndex = 0;
  }

  /**
   * Bind input event listener to the text field.
   * Sends current text value on every change.
   */
  bindEvents() {
    const text = this.container.querySelector('#myText');

    text.addEventListener('input', (event) => {
      const value = event.target.value;
      socketManager.updateText(value);

      const target = (this.words[this.currentIndex] || "").toLowerCase();
      console.log(target);
      if (value.toLowerCase() === target) {
        console.log("GOT IT");
        text.value = "";
        text.focus();
        this.currentIndex++;
      }
    });
  }
}
