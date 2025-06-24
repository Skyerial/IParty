import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

/**
 * ButtonsComponent
 *
 * Renders a configurable set of buttons and handles touch interactions for each.
 * Buttons are grouped into clusters based on the number provided (2 or 4 by default).
 */
export class ButtonsComponent extends ViewRenderer {
  /**
   * Create a ButtonsComponent
   * 
   * @param {HTMLElement} container - Element where the buttons view will be rendered.
   * @param {boolean} [vertical=false] - If true, arrange button clusters vertically.
   * @param {string[]} [buttons=['A','B','C','D']] - Array of button labels to display.
   */
  constructor(container, vertical = false, buttons = []) {
    super("./views/components/buttonsComponentView.html", container);
    this.vertical = vertical;
    this.buttons = buttons.length > 0 ? buttons : ['A', 'B', 'C', 'D'];
  }

  /**
   * After the view loads, initializes the button layout, applies orientation,
   * and sets up touch event listeners for each button to send updates over the socket.
   */
  bindEvents() {
    this.initLayout();

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

  /**
   * Creates clusters of buttons and appends them to the container.
   * Uses a specific CSS class based on the number of buttons.
   */
  initLayout() {
    if (this.buttons.length === 0) return;
    const clusterClass = this.determineClusterClass();
    const container = this.container.querySelector('.button-container');
    const cluster = document.createElement('div');
    cluster.classList.add(clusterClass);

    this.buttons.forEach(label => {
      const buttonElement = document.createElement('button');
      buttonElement.classList.add('game-button', `btn-${label.toLowerCase()}`);
      buttonElement.textContent = label.toUpperCase();
      cluster.appendChild(buttonElement);
    });

    container.appendChild(cluster);
  }

  /**
   * Determines the CSS class for the button cluster based on count.
   * @returns {string} Cluster class name ('button-cluster' or 'button-cluster-two').
   */
  determineClusterClass() {
    switch(this.buttons.length) {
      case 4:
        return "button-cluster";
      case 2:
        return "button-cluster-two";
      default:
        return "button-cluster";
    }
  }
}
