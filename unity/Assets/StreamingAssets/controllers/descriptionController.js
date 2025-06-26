import { LayoutManager } from "../utils/layoutManager.js";
import { ListComponent } from "./components/listComponent.js";

export class DescriptionController {
  /**
   * Create a DescriptionController.
   *
   * @param {HTMLElement} container - The DOM element to host the D-pad and buttons.
   */
  constructor(container) {
    this.container = container;
    this.currBtnName = "Controls";
  }

  /**
   * Initialize the layout by creating a LayoutManager and adding controls.
   */
  async init() {
    const layout = new LayoutManager(this.container, false);
    await layout.init();

    await layout.addList([
      { itemName: "Controls", itemColor: "#191970", connectedBtn: "A" },
      { itemName: "Start", itemColor: "#006400", connectedBtn: "C" },
    ]);

    this.listComponent = layout.getComponent(ListComponent);
    this.boundChange = this.changeButtonName.bind(this);

    this.listComponent.addItemListener(
      "Controls",
      "click",
      this.boundChange
    );
  }

  changeButtonName() {
    const oldName = this.currBtnName;
    const newName = oldName === "Controls" ? "Description" : "Controls";
    this.listComponent.changeItemName(oldName, newName);
    this.currBtnName = newName;
  }
}
