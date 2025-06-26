import { LayoutManager } from "../utils/layoutManager.js";
import { ListComponent } from "./components/listComponent.js";

/**
 * HotPotatoController initializes a list-based game UI using player statistics.
 *
 * It uses LayoutManager to append a list component populated with player names,
 * and appropriate style based on player stats 
 */
export class HotPotatoController {
  /**
   * Create a HotPotatoController.
   *
   * @param {HTMLElement} container - The DOM element to host the list component.
   * @param {Array<Object>} [playerStats=[]] - Array of players to display.
   */
  constructor(container, playerStats = []) {
    this.container = container;
    this.playerStats = playerStats;
  }

  /**
   * Initialize the layout by creating a LayoutManager and adding a list.
   */
  async init() {
    this.layout = new LayoutManager(this.container, true);
    await this.layout.init();

    // Add a list populated with playernames
    await this.layout.addList(this.playerStats);
  }

  /**
   * Remove a player’s button from the on-screen list.
   *
   * Looks up the ListComponent in this controller’s layout, and if found
   * hides (removes) the entry matching `playerName`.
   *
   * @param {string} playerName - The exact label of the player to remove.
   */
  removePlayer(playerName) {
    const listComponent = this.layout.getComponent(ListComponent);
    if (listComponent) {
      listComponent.removeItem(playerName);
    }
  }
}
