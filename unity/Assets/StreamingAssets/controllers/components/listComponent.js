import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from "../../main.js";

/**
 * @brief Renders a customizable list of touch-responsive buttons 
 *        and emits button press/release events through the socketManager.
 */
export class ListComponent extends ViewRenderer {
  /**
   * @brief Constructs a ListComponent.
   * @param {HTMLElement} container - The DOM element to render the list into.
   * @param {boolean} vertical - If true, layout the list vertically; otherwise horizontally.
   * @param {Array<Object>} [items=[]] - Initial list of players, each with:
   *   @param {string} items.itemName - Text label displayed on the button.
   *   @param {string} items.itemColor - Background color of the button.
   *   @param {string} items.connectedBtn - Button identifier sent on events.
   */
  constructor(container, vertical, items = []) {
    super("./views/components/listComponentView.html", container);
    this.items = items;
    this.vertical = vertical;
  }

  /**
   * @brief Binds pointer event listeners and renders buttons.
   * @details Clears previous content and creates a button for each item,
   *          wiring touch events to socket updates.
   */
  bindEvents() {
    const listContainer = this.container.querySelector(".player-button-list");
    listContainer.innerHTML = "";

    this.items.forEach(({ itemName, itemColor, connectedBtn }) => {
      const button = document.createElement("button");
      button.classList.add("player-list-button", `${itemName}`);
      button.style.backgroundColor = itemColor;
      button.textContent = itemName;

      button.addEventListener("pointerdown", (e) => {
        if (e.pointerType !== "touch") return;
        socketManager.updateButton(connectedBtn, true);
        button.setPointerCapture(e.pointerId);
      });

      button.addEventListener("pointerup", (e) => {
        if (e.pointerType !== "touch") return;
        socketManager.updateButton(connectedBtn, false);
        button.releasePointerCapture(e.pointerId);
      });

      button.addEventListener("pointercancel", (e) => {
        if (e.pointerType !== "touch") return;
        socketManager.updateButton(connectedBtn, false);
      });

      listContainer.appendChild(button);
    });
  }

  /**
   * @brief Removes the given item from the UI.
   * @param {string} itemName - The name of the item to remove (matches initial itemName).
   * @returns {void}
   */
  removeItem(itemName) {
    const item = this.container.querySelector(`.${itemName}`);
    if (!item) return;
    item.style.display = "none";
  }

  /**
   * @brief Attaches an event listener to a specific item's button.
   * @param {string} itemName - The name of the item (must match CSS class).
   * @param {string} eventType - The DOM event type (e.g., 'click', 'pointerdown').
   * @param {Function} handler - The listener function receiving the Event object.
   * @returns {boolean} True if the button was found and listener attached; false otherwise.
   */
  addItemListener(itemName, eventType, handler) {
    const btn = this.container.querySelector(`.player-list-button.${itemName}`);
    if (!btn) return false;
    btn.addEventListener(eventType, handler);
    return true;
  }
  
  /**
   * @brief Changes the label and CSS class of one of the buttons.
   * @param {string} oldName - The existing itemName.
   * @param {string} newName - The new itemName to replace it with.
   * @returns {boolean} True if the button was found and renamed; false otherwise.
   */
  changeItemName(oldName, newName) {
    const btn = this.container.querySelector(`.player-list-button.${oldName}`);
    if (!btn) return false;

    btn.textContent = newName;
    btn.classList.replace(oldName, newName);

    const item = this.items.find(i => i.itemName === oldName);
    if (item) item.itemName = newName;

    return true;
  }
}
