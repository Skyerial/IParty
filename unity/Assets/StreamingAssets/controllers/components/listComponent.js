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
   *   @param {string} items.itemName - Text label displayed on the button.
   *   @param {string} items.itemColor - Background color of the button (CSS color string).
   *   @param {string} items.connectedBtn - Button sent on button events.
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

    if (!item) return;
    item.style.display = "none";
  }

  /**
   * Attach an event listener to a specific item’s button.
   *
   * @param {string} itemName        - The name of the item (must match the CSS class you passed into bindEvents).
   * @param {string} eventType       - The DOM event type (e.g. 'click', 'pointerdown').
   * @param {Function} handler       - The listener function (receives the Event object).
   * @returns {boolean}              - True if the button was found & listener attached; false otherwise.
   */
  addItemListener(itemName, eventType, handler) {
    const btn = this.container.querySelector(`.player-list-button.${itemName}`);
    if (!btn) return false;
    btn.addEventListener(eventType, handler);
    return true;
  }
  
  /**
   * Change the label (and identifying CSS class) of one of your buttons.
   *
   * @param {string} oldName  - the existing itemName
   * @param {string} newName  - the new itemName to replace it with
   * @returns {boolean}       - true if the button was found & renamed
   */
  changeItemName(oldName, newName) {
    const btn = this.container.querySelector(
      `.player-list-button.${oldName}`
    );
    if (!btn) return false;

    btn.textContent = newName;
    btn.classList.replace(oldName, newName);

    const item = this.items.find(i => i.itemName === oldName);
    if (item) item.itemName = newName;

    return true;
  }
}
