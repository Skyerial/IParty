import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from '../../main.js';

export class ButtonComponent extends ViewRenderer{
  constructor(container) {
    super("./views/components/buttonComponentView.html", container);
  }

  bindEvents() {
    const btn = this.container.querySelector('.big-button');
    btn.addEventListener('touchstart', () => socketManager.updateButton('button', true));
    btn.addEventListener('touchend',   () => socketManager.updateButton('button', false));
  }
}
