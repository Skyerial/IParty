import { ControllerLayout } from "./controllerLayout.js";

export class JoystickController {
    constructor(container) {
        this.container = container;
    }

    async init() {
        const layout = new ControllerLayout(this.container, false);
        await layout.init();

        await layout.addJoystick();
        await layout.addButtons();
    }
}