import { LayoutManager } from "../utils/layoutManager";

export class bombController {
    constructor(container) {
        this.container = container;
    }

    async init() {
        const layout = new LayoutManager(this.container, true);
        await layout.init();
        await layout.addButtons();
    }
}