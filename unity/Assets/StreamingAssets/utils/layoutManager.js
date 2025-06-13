import { socketManager } from "../main.js";
import { ButtonComponent } from "../controllers/components/buttonComponent.js";
import { ButtonsComponent } from "../controllers/components/buttonsComponent.js";
import { DpadComponent } from "../controllers/components/dpadComponent.js";
import { JoystickComponent } from "../controllers/components/joystickComponent.js";
import { ViewRenderer } from "./viewRenderer.js";

export class LayoutManager extends ViewRenderer {
    constructor(container, vertical = false) {
        super("./views/controllerView.html", container);
        this.slots = [];
        this.vertical = vertical;
        this.hasMovementComponent = false;
    }

    bindEvents() {
        const layout = this.container.querySelector('.controller-layout');
        this.layout = layout;
    }

    async addDpad() {
        if (this.hasMovementComponent){
            alert("You can only use a joystick or a D-pad, not both.");
            return;
        }

        socketManager.changeMovementType("dpad");
        await this.addComponent(DpadComponent);
        this.hasMovementComponent = true;
    }

    async addJoystick() {
        if (this.hasMovementComponent){
            alert("You can only use a joystick or a D-pad, not both.");
            return;
        }

        socketManager.changeMovementType("analog");
        await this.addComponent(JoystickComponent);
        this.hasMovementComponent = true;
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