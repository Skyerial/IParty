import { LayoutManager } from "../utils/layoutManager.js";

export class DpadController {
    constructor(container) {
        this.container = container;
        setCurrentController(this);
    }

    async init() {
        const layout = new LayoutManager(this.container, false);
        await layout.init();

        await layout.addDpad();
        await layout.addButtons();
    }
}