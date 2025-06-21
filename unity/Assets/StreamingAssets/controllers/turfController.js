import { LayoutManager } from "../utils/layoutManager.js";

export class TurfController {
    constructor(container) {
        this.container = container;
    }

    async init() {
        const layout = new LayoutManager(this.container, false);
        await layout.init();

        await layout.addJoystick();
        await layout.addButtons(true);
    }
}