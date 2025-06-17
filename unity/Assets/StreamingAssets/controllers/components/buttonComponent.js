// ButtonComponent.js
import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from '../../main.js';

export class ButtonComponent extends ViewRenderer {
  constructor(container) {
    super("./views/components/buttonComponentView.html", container);
  }

  bindEvents() {
    const btn = this.container.querySelector('.big-button');

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
