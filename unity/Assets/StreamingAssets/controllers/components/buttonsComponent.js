import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class ButtonsComponent extends ViewRenderer {
  constructor(container, vertical = false, buttons = []) {
    let htmlPath = "./views/components/buttonsComponentView.html";

    super(htmlPath, container);
    this.vertical = vertical;

    this.buttons = (buttons.length < 1) ?  ["A", "B", "C", "D"] : buttons;
  }

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

  initLayout() {
    if(this.buttons.length < 1) {
      return;
    }
    const cluserClass = this.determineClusterClass();

    const container = this.container.querySelector(".button-container");
    const cluster = document.createElement("div");
    cluster.classList.add(cluserClass);

    this.buttons.forEach(button => {
      const buttonElement = document.createElement("button");
      buttonElement.classList.add(`game-button`, `btn-${button.toLowerCase()}`);
      buttonElement.innerHTML = button.toUpperCase();

      cluster.appendChild(buttonElement);
    })

    container.appendChild(cluster);
  }

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
