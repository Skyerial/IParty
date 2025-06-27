import { LayoutManager } from "../utils/layoutManager.js";

/**
 * @brief Controller class for managing a tank joystick interface.
 * @details Initializes a layout with a joystick and a single control button.
 */
export class TankJoystickController {
    /**
     * @brief Creates an instance of TankJoystickController.
     * @param {HTMLElement} container - The HTML container element to render the joystick UI.
     */
    constructor(container) {
        this.container = container;
    }

    /**
     * @brief Initializes the layout and adds joystick and button controls to the container.
     * @returns {Promise<void>} Resolves when the layout, joystick, and button have been rendered.
     */
    async init() {
        const layout = new LayoutManager(this.container, false);
        await layout.init();

        await layout.addJoystick();
        await layout.addButtons(["A"]);
    }
}
