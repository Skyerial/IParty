import { ControllerLayout } from "./controllerLayout.js";

export class DpadController {
    constructor(container) {
        this.container = container;
    }

    async init() {
        const layout = new ControllerLayout(this.container, false);
        await layout.init();

        await layout.addDpad();
        await layout.addButtons();
    }
}