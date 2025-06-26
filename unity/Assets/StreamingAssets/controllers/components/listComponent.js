import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from "../../main.js";

/**
 * ListComponent renders a customizable list of touch-responsive buttons
 * and emits button press/release events through the socketManager.

 */
export class ListComponent extends ViewRenderer {
  /**
   * Create a ListComponent.
   *
   * @param {HTMLElement} container - The DOM element to render the list into.
   * @param {boolean} vertical - If true, layout the list vertically; otherwise horizontally.
   * @param {Array<Object>} [items=[]] - Initial list of players.
   *   Each item should be an object with the following properties:
   *   @param {string} items.label - Text label displayed on the button.
   *   @param {string} items.color - Background color of the button (CSS color string).
   *   @param {string} items.jsonButton - Button sent on button events.
   */
  constructor(container, vertical, items = []) {
    super("./views/components/listComponentView.html", container);
    this.items = items;
    this.vertical = vertical;
  }

  /**
   * Bind pointer event listeners and render buttons.
   * Clears previous content and creates a button for each item,
   * wiring touch events to socket updates.
   */
  bindEvents() {
    const listContainer = this.container.querySelector(".player-button-list");
    listContainer.innerHTML = "";

    // Render each item as a touchable button
    this.items.forEach(({ itemName, itemColor, connectedBtn }) => {
      const button = document.createElement("button");
      button.classList.add("player-list-button", `${itemName}`);
      button.style.backgroundColor = itemColor;
      button.textContent = itemName;

      // On touch start: send press event
      button.addEventListener("pointerdown", (e) => {
        if (e.pointerType !== "touch") return;
        socketManager.updateButton(connectedBtn, true);
        button.setPointerCapture(e.pointerId);
      });

      // On touch end: send release event
      button.addEventListener("pointerup", (e) => {
        if (e.pointerType !== "touch") return;
        socketManager.updateButton(connectedBtn, false);
        button.releasePointerCapture(e.pointerId);
      });

      // On touch cancel: send release event
      button.addEventListener("pointercancel", (e) => {
        if (e.pointerType !== "touch") return;
        socketManager.updateButton(connectedBtn, false);
      });

      listContainer.appendChild(button);
    });
  }

  /**
   * Removes the given item from the UI.
   * 
   * @param {string} itemName - The name of the item you want to remove.
   * This name has to be the same as the one passed to the list component initially.
   */
  removeItem(itemName) {
    const item = this.container.querySelector(`.${itemName}`);

    if (!player) return;
    item.style.display = "none";
  }
}
