import { LayoutManager } from "../utils/layoutManager.js";

export class JoystickController {
    constructor(container) {
        this.container = container;
    }

    async init() {
        const layout = new LayoutManager(this.container, true);
        await layout.init();

        await layout.addDpad();
        await layout.addButtons();
    }
}