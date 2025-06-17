import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class ButtonsComponent extends ViewRenderer {
  constructor(container, vertical = false) {
    super(
      "./views/components/buttonsComponentView.html",
      container,
      "buttonInput"
    );
    this.vertical = vertical;
  }

  bindEvents() {
    if (this.vertical) {
      this.container
        .querySelector('.button-container')
        .classList.add('orientation-vertical');
    }

    this.container.querySelectorAll('.game-button').forEach(button => {
      const name = button.innerText;

      button.addEventListener('pointerdown', e => {
        if (e.pointerType !== 'touch') return;
        socketManager.updateButton(name, true);
        button.setPointerCapture(e.pointerId);
      });

      button.addEventListener('pointerup', e => {
        if (e.pointerType !== 'touch') return;
        socketManager.updateButton(name, false);
        button.releasePointerCapture(e.pointerId);
      });

      button.addEventListener('pointercancel', e => {
        if (e.pointerType !== 'touch') return;
        socketManager.updateButton(name, false);
      });
    });
  }
}
