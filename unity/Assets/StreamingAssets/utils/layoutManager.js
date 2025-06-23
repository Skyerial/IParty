import { socketManager } from "../main.js";
import { ButtonComponent } from "../controllers/components/buttonComponent.js";
import { ButtonsComponent } from "../controllers/components/buttonsComponent.js";
import { DpadComponent } from "../controllers/components/dpadComponent.js";
import { JoystickComponent } from "../controllers/components/joystickComponent.js";
import { TextComponent } from "../controllers/components/textComponent.js";
import { ViewRenderer } from "./viewRenderer.js";
import { ListComponent } from "../controllers/components/listComponent.js";
import { GyroComponent } from "../controllers/components/gyroComponent.js";

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

    async addText() {
        socketManager.changeMovementType("text");
        await this.addComponent(TextComponent)
    }

    async addButtons() {
        await this.addComponent(ButtonsComponent);
    }

    async addButton(items = {}) {
        await this.addComponent(ButtonComponent, items);
    }

    async addList(items = []) {
        await this.addComponent(ListComponent, items);
    }

    async addGyro() {
        await this.addComponent(GyroComponent);
    }

    async addComponent(ComponentClass, param = null) {
        const root = document.createElement("div");

        let component;
        if(param) {
            component = new ComponentClass(root, this.vertical, param);
        } else {
            component = new ComponentClass(root, this.vertical);
        }

        if (component instanceof ListComponent) {
            root.classList.add("controller-slot-list");
        }

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