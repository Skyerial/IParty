import { socketManager } from "../main.js";
import { ButtonComponent } from "../controllers/components/buttonComponent.js";
import { ButtonsComponent } from "../controllers/components/buttonsComponent.js";
import { DpadComponent } from "../controllers/components/dpadComponent.js";
import { JoystickComponent } from "../controllers/components/joystickComponent.js";
import { TextComponent } from "../controllers/components/textComponent.js";
import { ViewRenderer } from "./viewRenderer.js";
import { ListComponent } from "../controllers/components/listComponent.js";
import { GyroComponent } from "../controllers/components/gyroComponent.js";

/**
 * LayoutManager
 *
 * Builds a controller interface layout by adding input components into slots.
 * Supports only one movement component (D-pad or joystick) at a time.
 */
export class LayoutManager extends ViewRenderer {
    /**
     * Create a LayoutManager
     * 
     * @param {HTMLElement} container - Element where controller slots will be added.
     * @param {boolean} vertical - If true, arrange components vertically.
     */
    constructor(container, vertical = false) {
        super("./views/controllerView.html", container);
        this.slots = [];
        this.vertical = vertical;
        this.hasMovementComponent = false;
        this.components = [];
    }

    /**
     * Called after HTML loads: finds the layout element.
     */
    bindEvents() {
        this.layout = this.container.querySelector('.controller-layout');
    }

    /**
     * Adds a D-pad component if none exists.
     */
    async addDpad() {
        if (!this.startMovement('dpad')) return;
        await this.addComponent(DpadComponent);
    }

    /**
     * Adds a joystick component if none exists.
     */
    async addJoystick() {
        if (!this.startMovement('analog')) return;
        await this.addComponent(JoystickComponent);
    }

    /**
     * Adds a text input component if none exists.
     */
    async addText(words) {
        if (!this.startMovement('text')) return;
        await this.addComponent(TextComponent, words);
    }

    /**
     * Adds multiple buttons from an array of settings.
     * @param {string[]} buttons
     */
    async addButtons(buttons = []) {
        await this.addComponent(ButtonsComponent, buttons);
    }

    /**
     * Adds a single button with custom settings.
     * @param {object} items
     */
    async addButton(items = {}) {
        await this.addComponent(ButtonComponent, items);
    }

    /**
     * Adds a list view for stats or items.
     * @param {Array<object>} items
     */
    async addList(items = []) {
        await this.addComponent(ListComponent, items);
    }

    /**
     * Adds a gyroscope-based component.
     * 
     * @param {string} displayText - The text to be displayed.
     */
    async addGyro(displayText) {
        await this.addComponent(GyroComponent, displayText);
    }

    /**
     * Helper: ensures only one movement component, sets mode, and marks usage.
     * @param {string} type - 'dpad' or 'analog'
     * @returns {boolean} True if allowed, false if already used.
     */
    startMovement(type) {
        if (this.hasMovementComponent) {
            alert('Only one movement component allowed');
            return false;
        }
        socketManager.changeMovementType(type);
        this.hasMovementComponent = true;
        return true;
    }

    /**
     * Creates, initializes, and displays a component in a new slot.
     * @param {Function} ComponentClass
     * @param {*} [param]
     */
    async addComponent(ComponentClass, param = null) {
        const root = document.createElement('div');
        const component = param
            ? new ComponentClass(root, this.vertical, param)
            : new ComponentClass(root, this.vertical);

        if (component instanceof ListComponent) {
            root.classList.add('controller-slot-list');
        }

        await component.init();
        this.components.push(component);
        this.loadComponentView(component);
    }

    /**
     * Wraps a component in a slot and adds it to the layout.
     * @param {object} component
     */
    loadComponentView(component) {
        const slot = document.createElement('div');
        slot.className = 'controller-slot';
        slot.appendChild(component.getContainer());
        this.layout.appendChild(slot);
        this.slots.push(slot);
    }

    /**
     * Returns the first component of the given class.
     * 
     * @param {object} componentClass - The class of the component you're looking for
     * @returns The component you're looking for
     */
    getComponent(componentClass) {
        return this.components.find(c => c instanceof componentClass) || null;
    }
}