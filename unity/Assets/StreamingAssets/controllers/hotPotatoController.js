import { LayoutManager } from "../utils/layoutManager.js";

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
    const layout = new LayoutManager(this.container, true);
    await layout.init();

    // Add a list populated with playernames
    await layout.addList(this.playerStats);
  }
}
