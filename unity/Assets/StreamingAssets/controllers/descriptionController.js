import { LayoutManager } from "../utils/layoutManager.js";
import { ListComponent } from "./components/listComponent.js";

/**
 * @brief Controller that displays and toggles between "Controls" and "Description" items in a list.
 * @details Uses LayoutManager to render the list and handles click events to swap item names.
 */
export class DescriptionController {
  /**
   * @brief Constructs a DescriptionController.
   * @param {HTMLElement} container - The DOM element to host the list component.
   */
  constructor(container) {
    this.container = container;
    this.currBtnName = "Controls";
  }

  /**
   * @brief Initializes the layout by creating a LayoutManager and adding the list of items.
   * @returns {Promise<void>} Resolves when the layout and event listeners are set up.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    // Add list with two items: Controls and Start
    await layout.addList([
      { itemName: "Controls", itemColor: "#191970", connectedBtn: "A" },
      { itemName: "Start",    itemColor: "#006400", connectedBtn: "C" },
    ]);

    // Retrieve the ListComponent instance for later updates
    this.listComponent = layout.getComponent(ListComponent);

    // Bind the changeButtonName method to preserve 'this'
    this.boundChange = this.changeButtonName.bind(this);

    // Attach click listener to the "Controls" item
    this.listComponent.addItemListener(
      "Controls",
      "click",
      this.boundChange
    );
  }

  /**
   * @brief Toggles the displayed item name between "Controls" and "Description".
   * @details Updates both the UI label and the stored current name.
   */
  changeButtonName() {
    const oldName = this.currBtnName;
    const newName = oldName === "Controls" ? "Description" : "Controls";
    this.listComponent.changeItemName(oldName, newName);
    this.currBtnName = newName;
  }
}
