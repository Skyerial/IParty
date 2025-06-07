import { ButtonComponent } from "./components/buttonComponent.js";
import { ButtonsComponent } from "./components/buttonsComponent.js";
import { DpadComponent } from "./components/dpadComponent.js";
import { JoystickComponent } from "./components/joystickComponent.js";
import { Controller } from "./controller.js";

export class ControllerLayout extends Controller {
    constructor(container, vertical = false) {
        super("../views/controllerView.html",container);
        this.slots = [];
        this.vertical = vertical;
    }

    bindEvents() {
        const layout = this.container.querySelector('.controller-layout');
        this.layout = layout;
    }

    async addDpad() {
        await this.addComponent(DpadComponent);
    }

    async addJoystick() {
        await this.addComponent(JoystickComponent);
    }

    async addButtons() {
        await this.addComponent(ButtonsComponent);
    }

    async addButton() {
        await this.addComponent(ButtonComponent);
    }

    async addComponent(ComponentClass) {
        const root = document.createElement("div");
        const component = new ComponentClass(root, this.vertical);
        await component.init();
        this.loadComponentView(component);
    }

    loadComponentView(component) {
        let container = component.getContainer();
        const slot = document.createElement('div');
        slot.className = 'controller-slot';
        
        slot.appendChild(container);
        this.layout.appendChild(slot);
        this.slots.push(slot);
    }
}