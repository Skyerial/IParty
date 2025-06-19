import { LayoutManager } from "../utils/layoutManager.js";

export class HotPotatoController {
    constructor(container, playerStats = []) {
        this.container = container;
        this.playerStats = playerStats;
    }

    async init() {
        const layout = new LayoutManager(this.container, true);
        await layout.init();
        await layout.addList(this.playerStats);
    }
}