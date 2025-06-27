import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

/**
 * @brief Renders a D-pad input control and handles touch interactions for directional input.
 * @details Supports optional vertical orientation.
 */
export class DpadComponent extends ViewRenderer {
  /**
   * @brief Constructs a DpadComponent instance.
   * @param {HTMLElement} container - Element where the D-pad view will be rendered.
   * @param {boolean} [vertical=false] - If true, arranges the D-pad vertically.
   */
  constructor(container, vertical = false) {
    super("./views/components/dpadComponentView.html", container, "dpadInput");
    this.vertical = vertical;
  }

  /**
   * @brief Called after the view loads; applies vertical orientation if needed,
   *        then sets up touch event listeners on each direction button
   *        to send D-pad updates over the socket.
   */
  bindEvents() {
    if (this.vertical) {
      this.container
        .querySelector('.dpad-container')
        .classList.add('orientation-vertical');
    }

    ['up', 'down', 'left', 'right'].forEach(dir => {
      const btn = this.container.querySelector(`.dpad-${dir}`);
      btn.addEventListener('touchstart', () => socketManager.updateDpad(dir, true));
      btn.addEventListener('touchend',   () => socketManager.updateDpad(dir, false));
    });
  }
}
