import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from '../../main.js';

/**
 * @brief Renders a single large button and handles touch interactions.
 * @details Supports custom text and CSS class via options, and sends button-press updates over the socket.
 */
export class ButtonComponent extends ViewRenderer {
  /**
   * @brief Constructs a ButtonComponent instance.
   * @param {HTMLElement} container - Element where the button view will be rendered.
   * @param {boolean} vertical - If true, button is laid out vertically.
   * @param {Object} [options] - Configuration options for the button.
   * @param {string} [options.text] - Label text to display inside the button.
   * @param {string} [options.className] - Additional CSS class to apply to the button.
   */
  constructor(container, vertical, options = {}) {
    super("./views/components/buttonComponentView.html", container);
    this.options = options;
    this.vertical = vertical;
  }

  /**
   * @brief Called after the view loads; finds the button element, applies custom text and class,
   *        and sets up touch event listeners to send button-press updates over the socket.
   */
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
