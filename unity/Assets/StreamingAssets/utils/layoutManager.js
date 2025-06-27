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
 * @brief Builds and manages the layout of controller input components in the UI.
 *        Ensures only one movement component is added.
 */
export class LayoutManager extends ViewRenderer {
    /**
     * @brief Constructs a LayoutManager.
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
     * @brief Called after HTML is loaded; locates the controller-layout element.
     */
    bindEvents() {
        this.layout = this.container.querySelector('.controller-layout');
    }

    /**
     * @brief Adds a D-pad component if none exists.
     * @returns {Promise<void>}
     */
    async addDpad() {
        if (!this.startMovement('dpad')) return;
        await this.addComponent(DpadComponent);
    }

    /**
     * @brief Adds a joystick (analog) component if none exists.
     * @returns {Promise<void>}
     */
    async addJoystick() {
        if (!this.startMovement('analog')) return;
        await this.addComponent(JoystickComponent);
    }

    /**
     * @brief Adds a text input component.
     * @param {string} words - Initial text or placeholder for the TextComponent.
     * @returns {Promise<void>}
     */
    async addText(words) {
        if (!this.startMovement('text')) return;
        await this.addComponent(TextComponent, words);
    }

    /**
     * @brief Adds multiple buttons based on an array of settings.
     * @param {string[]} buttons - Array of button labels or configurations.
     * @returns {Promise<void>}
     */
    async addButtons(buttons = []) {
        await this.addComponent(ButtonsComponent, buttons);
    }

    /**
     * @brief Adds a single button with custom settings.
     * @param {object} items - Configuration object for the button.
     * @returns {Promise<void>}
     */
    async addButton(items = {}) {
        await this.addComponent(ButtonComponent, items);
    }

    /**
     * @brief Adds a list component for displaying collections of items or stats.
     * @param {Array<object>} items - Items to populate the list.
     * @returns {Promise<void>}
     */
    async addList(items = []) {
        await this.addComponent(ListComponent, items);
    }

    /**
     * @brief Adds a gyroscope-based control component.
     * @param {string} displayText - Text to display alongside the GyroComponent.
     * @returns {Promise<void>}
     */
    async addGyro(displayText) {
        await this.addComponent(GyroComponent, displayText);
        socketManager.changeMovementType('analog');
    }

    /**
     * @brief Ensures only one movement component (D-pad or joystick/text) is added.
     * @param {string} type - Movement type: 'dpad', 'analog', or 'text'.
     * @returns {boolean} True if movement component can be added; false otherwise.
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
     * @brief Creates, initializes, and displays a component instance in a new slot.
     * @param {Function} ComponentClass - The component class to instantiate.
     * @param {*} [param] - Optional parameter passed to the component constructor.
     * @returns {Promise<void>}
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
     * @brief Wraps a component in a slot element and appends it to the layout.
     * @param {object} component - The component instance whose view to load.
     * @returns {void}
     */
    loadComponentView(component) {
        const slot = document.createElement('div');
        slot.className = 'controller-slot';
        slot.appendChild(component.getContainer());
        this.layout.appendChild(slot);
        this.slots.push(slot);
    }

    /**
     * @brief Retrieves the first component instance of the specified class.
     * @param {Function} componentClass - The component class to search for.
     * @returns {object|null} The matching component instance, or null if not found.
     */
    getComponent(componentClass) {
        return this.components.find(c => c instanceof componentClass) || null;
    }
}
