import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class ButtonsComponent extends ViewRenderer{
  constructor(container, vertical = false) {
    super("./views/components/buttonsComponentView.html", container, "buttonInput");
    this.vertical = vertical;
  }

  bindEvents() {
    if (this.vertical) {
      this.container.querySelector('.button-container')
                    .classList.add('orientation-vertical');
    }

    this.container.querySelectorAll('.game-button').forEach(button => {
      const name = button.innerText;
      button.addEventListener('touchstart', () => socketManager.updateButton(name, true));
      button.addEventListener('touchend',   () => socketManager.updateButton(name, false));
    });
  }
}
