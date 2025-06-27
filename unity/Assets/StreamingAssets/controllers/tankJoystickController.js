import { LayoutManager } from "../utils/layoutManager.js";

/**
 * Controller class for managing a tank joystick interface.
 * Initializes a layout with a joystick and control buttons.
 */
export class TankJoystickController {
    /**
     * Creates an instance of TankJoystickController.
     * @param {HTMLElement} container - The HTML container element to render the joystick UI.
     */
    constructor(container) {
        this.container = container;
    }

    /**
     * Initializes the layout and adds joystick and button controls to the container.
     * - Creates a LayoutManager instance with the provided container.
     * - Initializes the layout.
     * - Adds a joystick control.
     * - Adds a single button labeled "A".
     * 
     * @returns {Promise<void>}
     */
    async init() {
        const layout = new LayoutManager(this.container, false);
        await layout.init();

        await layout.addJoystick();
        await layout.addButtons(["A"]);
    }
}