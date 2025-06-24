import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from "../../main.js";

export class ListComponent extends ViewRenderer {
  constructor(container, vertical, items = []) {
    super("./views/components/listComponentView.html", container);
    this.items = items;
    this.vertical = vertical;
    console.log(items.toString());

  }

  bindEvents() {
    const listContainer = this.container.querySelector(".player-button-list");
    listContainer.innerHTML = '';

    this.items.forEach(({ label, color, jsonButton }) => {
      const button = document.createElement("button");
      button.classList.add("player-list-button");
      button.style.backgroundColor = color;
      button.textContent = label;

      button.addEventListener("pointerdown", e => {
        if (e.pointerType !== 'touch') return;
        socketManager.updateButton(jsonButton, true);
        button.setPointerCapture(e.pointerId);
      });

      button.addEventListener("pointerup", e => {
        if (e.pointerType !== 'touch') return;
        socketManager.updateButton(jsonButton, false);
        button.releasePointerCapture(e.pointerId);
      });

      button.addEventListener("pointercancel", e => {
        if (e.pointerType !== 'touch') return;
        socketManager.updateButton(jsonButton, false);
      });

      listContainer.appendChild(button);
    });
  }
}
