import { socketManager } from "../../main.js";
import { ViewRenderer } from "../../utils/viewRenderer.js";

export class TextComponent extends ViewRenderer{
  constructor(container, vertical = false) {
    super("./views/components/textComponentView.html", container, "textInput");
    this.vertical  = vertical;
    this.origin = { x: 0, y: 0 };
  }

  bindEvents() {
    const text = this.container.querySelector('#myText');

    text.addEventListener('input', function(event) {
        socketManager.updateText(event.target.value)
    })
  }
}
