import { LayoutManager } from "../utils/layoutManager.js";
import { ListComponent } from "./components/listComponent.js";

/**
 * @brief Initializes a list-based game UI using player statistics.
 * @details Uses LayoutManager to append a ListComponent populated with player names.
 */
export class HotPotatoController {
  /**
   * @brief Constructs a HotPotatoController.
   * @param {HTMLElement} container - The DOM element to host the list component.
   * @param {Array<Object>} [playerStats=[]] - Array of player objects to display, each with itemName, itemColor, and connectedBtn.
   */
  constructor(container, playerStats = []) {
    this.container = container;
    this.playerStats = playerStats;
  }

  /**
   * @brief Initializes the layout by creating a LayoutManager and adding the player stats list.
   * @returns {Promise<void>} Resolves when the layout and list are rendered.
   */
  async init() {
    this.layout = new LayoutManager(this.container, true);
    await this.layout.init();

    // Add a list populated with playerNames
    await this.layout.addList(this.playerStats);
  }

  /**
   * @brief Removes a player’s button from the on-screen list.
   * @param {string} playerName - The exact label of the player to remove.
   */
  removePlayer(playerName) {
    const listComponent = this.layout.getComponent(ListComponent);
    if (listComponent) {
      listComponent.removeItem(playerName);
    }
  }
}
