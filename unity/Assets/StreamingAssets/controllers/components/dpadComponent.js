import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class DpadComponent extends ViewRenderer{
  constructor(container, vertical = false) {
    super("./views/components/dpadComponentView.html", container, "dpadInput");
    this.vertical = vertical;
  }

  bindEvents() {
    if (this.vertical) {
      this.container.querySelector('.dpad-container')
                   .classList.add('orientation-vertical');
    }
    ['up', 'down', 'left', 'right'].forEach(dir => {
      const btn = this.container.querySelector(`.dpad-${dir}`);
      btn.addEventListener('touchstart', () => socketManager.updateDpad(dir, true));
      btn.addEventListener('touchend',   () => socketManager.updateDpad(dir, false));
    });
  }
}
