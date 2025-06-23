import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from '../../main.js';

export class ButtonComponent extends ViewRenderer {
  constructor(container, vertical, options = {}) {
    super("./views/components/buttonComponentView.html", container);
    this.options = options;
    this.vertical = vertical;
  }

  bindEvents() {
    const btn = this.container.querySelector('.big-button');

    if (this.options.className) {
      btn.classList.add(this.options.className);
    }

     if (this.options.text != null) {
       btn.textContent = this.options.text;
     }

    btn.addEventListener('pointerdown', e => {
      if (e.pointerType !== 'touch') return;
      socketManager.updateButton('button', true);
      btn.setPointerCapture(e.pointerId);
    });

    btn.addEventListener('pointerup', e => {
      if (e.pointerType !== 'touch') return;
      socketManager.updateButton('button', false);
      btn.releasePointerCapture(e.pointerId);
    });

    btn.addEventListener('pointercancel', e => {
      if (e.pointerType !== 'touch') return;
      socketManager.updateButton('button', false);
    });
  }
}
