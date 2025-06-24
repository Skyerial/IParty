/**
 * @file SocketManager.js
 * @brief WebSocket management and controller handling for game clients
 */

import { DpadController } from "../controllers/dpadController.js";
import { GyroController } from "../controllers/gyroController.js";
import { HotPotatoController } from "../controllers/hotPotatoController.js";
import { JoystickController } from "../controllers/joystickController.js";
import { TextController } from "../controllers/textController.js";
import { OneButton } from "../controllers/oneButton.js";
import { ReadyController } from "../controllers/readyController.js";
import { CustomJoystickController } from "../controllers/customJoystickController.js";

/**
 * @brief Mapping of controller types to their corresponding controller classes
 * @details Maps string identifiers to controller constructors for dynamic controller instantiation
 */
const CONTROLLER_MAP = {
  "dpad-preset": DpadController,
  "joystick-preset": JoystickController,
  "text-preset": TextController,
  "one-button": OneButton,
  "turf": CustomJoystickController,
  "spleef": CustomJoystickController,
  "ready": ReadyController,
  "gyro": GyroController
};

/**
 * @class SocketManager
 * @brief Manages WebSocket connections and handles game controller input/output
 * @details This class handles WebSocket communication with game servers, manages different
 *          controller types (analog/dpad), and processes various game commands and states.
 */
export class SocketManager {
    /**
     * @brief Constructor for SocketManager
     * @param relayHost The hostname for the relay server (default: 'iparty.duckdns.org')
     * @param movementType The initial movement type - 'analog' or 'dpad' (default: 'analog')
     */
    constructor(relayHost = 'iparty.duckdns.org', movementType = 'analog') {
        /** @brief WebSocket connection instance */
        this.socket = null;
        /** @brief Relay server hostname */
        this.relayHost = relayHost;
        /** @brief Update interval in milliseconds for sending data */
        this.interval = 10;
        /** @brief Timer for periodic updates */
        this.timer = null;
        /** @brief Flag indicating if continuous updates are active */
        this.isActive = false;

        /** @brief Current client name */
        this.clientName = null;

        /** @brief Callback function for connection open events */
        this.onOpen = null;
        /** @brief Callback function for message events */
        this.onMessage = null;
        /** @brief Callback function for connection close events */
        this.onClose = null;

        /** @brief Last message sent to prevent duplicate transmissions */
        this.lastSentMessage = null;
        this.changeMovementType(movementType);
    }

    /**
     * @brief Switch between 'analog' and 'dpad' movement types
     * @param type Movement type - 'analog' for joystick, 'dpad' for directional pad
     * @details Resets the state object structure based on the movement type
     */
    changeMovementType(type) {
        this.activeMovementType = type === 'dpad' ? 'dpad' : 'analog';
        /** @brief Current input state containing button and movement data */
        this.state = {
            type: type,
            A: false,
            B: false,
            C: false,
            D: false,
            button: false,
            T: "",
        };

        if (this.activeMovementType === 'analog') {
            Object.assign(this.state, { x: 0, y: 0 });
        } else {
            Object.assign(this.state, { up: false, down: false, left: false, right: false });
        }
    }

    /**
     * @brief Establish WebSocket connection to game server
     * @param code Game room code for connection
     * @details Creates WebSocket connection and sets up event handlers for open, message, and close events
     */
    connect(code) {
        if (this.socket) return;
        const isRemote = location.hostname === this.relayHost
        const url = !isRemote
            ? `wss://${location.hostname}:8181`
            : `wss://iparty.duckdns.org:5001/host/${code}/ws`;
        console.log(url)
        this.socket = new WebSocket(url);
        this.socket.onopen = () => { console.log('🟢 Connected'); };
        this.socket.onmessage = async (e) => {
            console.log(e.data);
            let rawData;

            if (e.data instanceof Blob) {
                rawData = await e.data.text();
            }
            else {
                rawData = e.data;  // Already a string
            }
            const data = JSON.parse(rawData);
            this.handleCommand(data);
        };
        this.socket.onclose = (e) => {
            console.log('🔴 Disconnected');
            console.log(e.code);
            console.log(e.reason);
            console.log(e.wasClean);
            this.socket = null;
        };
    }

    /**
     * @brief Load and initialize a game controller based on server data
     * @param data Controller configuration data from server
     * @details Dynamically instantiates the appropriate controller class based on the controller type
     */
    loadController(data) {
        const root = document.querySelector(".view-container");
        const { controller, playerstats = [] } = data;

        if (controller === "hotpotato") {
            const listItems = playerstats.map(({ name, color, button }) => ({
                label: name,
                color,
                jsonButton: button
            }));

            new HotPotatoController(root, listItems).init();
        } else if (CONTROLLER_MAP[controller]) {
            new CONTROLLER_MAP[controller](root).init();
        } else {
            console.warn(`Unknown controller type: ${controller}`);
        }
    }

    /**
     * @brief Handle incoming commands from the server
     * @param data Parsed JSON data containing command information
     * @details Processes various command types including controller loading, text clearing,
     *          reconnection status, and character status
     */
    handleCommand(data) {
        let root = document.querySelector(".view-container");
        if (data.type == "controller") {
            console.log("loading controller")
            this.loadController(data)
        } else if (data.type == "clear-text") {
            const text = document.querySelector('#myText');
            text.value = ''
        } else if (data.type == "reconnect-status") {
            if (data.approved) {
                let js = new JoystickController(root)
                js.init()
            } else {
                alert("Player not found, please make sure you've entered the correct code.")
            }
        } else if (data.type == "character-status") {
            if (data.approved) {
                this.setClientName(data.name)
                let js = new JoystickController(root)
                js.init()
            } else {
                alert("Choose another color, this color is already chosen.")
            }
        }

        console.log(JSON.stringify(data));
    }

    /**
     * @brief Close the WebSocket connection
     * @details Safely closes the connection if it exists and is open
     */
    disconnect() {
        if (this.isConnected()) this.socket.close();
    }

    /**
     * @brief Check if WebSocket connection is currently open
     * @return True if connected, false otherwise
     */
    isConnected() {
        return this.socket?.readyState === WebSocket.OPEN;
    }

    /**
     * @brief Set event handler callbacks for WebSocket events
     * @param onOpen Callback for connection open event
     * @param onMessage Callback for message received event
     * @param onClose Callback for connection close event
     */
    setHandlers({ onOpen, onMessage, onClose }) {
        this.onOpen = onOpen;
        this.onMessage = onMessage;
        this.onClose = onClose;
    }

    /**
     * @brief Update analog stick position values
     * @param x Horizontal axis value (-1 to 1)
     * @param y Vertical axis value (-1 to 1)
     * @details Only processes updates if in analog movement mode and values have changed significantly
     */
    updateAnalog(x, y) {
        if (this.activeMovementType !== 'analog') return;

        const threshold = 0.01;
        const xChanged = Math.abs(x - this.state.x) > threshold;
        const yChanged = Math.abs(y - this.state.y) > threshold;

        if (!xChanged && !yChanged && (x !== 0 || y !== 0)) return;

        this.state.x = x;
        this.state.y = y;
        this.updateActivity();
    }

    /**
     * @brief Update directional pad button state
     * @param direction Direction key ('up', 'down', 'left', 'right')
     * @param value Boolean state of the direction button
     * @details Only processes updates if in dpad movement mode
     */
    updateDpad(direction, value) {
        if (this.activeMovementType !== 'dpad') return;

        this.state[direction] = value;
        if (this.isConnected()) {
            this.sendFiltered();
        }
    }

    /**
     * @brief Update button state
     * @param name Button name ('A', 'B', 'C', 'D', 'button')
     * @param value Boolean state of the button
     */
    updateButton(name, value) {
        this.state[name] = value;
        if (this.isConnected()) {
            this.sendFiltered();
        }
    }

    /**
     * @brief Update text input value
     * @param char Text character or string to send
     */
    updateText(char) {
        this.state.T = char
        if (this.isConnected()) {
            this.sendFiltered();
        }
    }

    /**
     * @brief Get filtered state object for transmission
     * @return Object containing only relevant state properties based on movement type
     * @details Combines base button states with movement-specific properties
     */
    getFilteredState() {
        const base = (({ type, A, B, C, D, button, T }) => ({ type, A, B, C, D, button, T }))(this.state);
        const movement = this.pick(this.state, this.getMovementKeys());
        return { ...base, ...movement };
    }

    /**
     * @brief Send filtered state to server if it has changed
     * @details Prevents sending duplicate messages by comparing with last sent message
     */
    sendFiltered() {
        if (!this.isConnected()) return;
        const message = JSON.stringify(this.getFilteredState());
        if (message === this.lastSentMessage) return; // Don't send duplicates
        this.lastSentMessage = message;
        console.log(message);
        this.socket.send(message);
    }

    /**
     * @brief Start periodic update loop for continuous data transmission
     * @details Used for analog input that requires continuous updates
     */
    startLoop() {
        if (this.timer) return;
        this.timer = setInterval(() => {
            if (this.isActive && this.isConnected()) {
                this.sendFiltered();
            }
        }, this.interval);
    }

    /**
     * @brief Stop periodic update loop
     */
    stopLoop() {
        clearInterval(this.timer);
        this.timer = null;
    }

    /**
     * @brief Update activity state based on analog input
     * @details Starts/stops continuous update loop based on whether analog input is active
     */
    updateActivity() {
        if (this.activeMovementType !== 'analog') return;

        const hasAnalog = this.state.x !== 0 || this.state.y !== 0;

        if (hasAnalog && !this.isActive) {
            this.isActive = true;
            this.startLoop();
        }
        else if (!hasAnalog && this.isActive) {
            this.isActive = false;
            this.stopLoop();
            if (this.isConnected()) {
            this.sendFiltered();
            }
        }
    }

    /**
     * @brief Get movement-related property keys based on current movement type
     * @return Array of property names for current movement type
     */
    getMovementKeys() {
        return this.activeMovementType === 'analog' ? ['x', 'y'] : ['up', 'down', 'left', 'right'];
    }

    /**
     * @brief Extract specific properties from an object
     * @param obj Source object
     * @param keys Array of property names to extract
     * @return New object containing only the specified properties
     */
    pick(obj, keys) {
        return keys.reduce((res, k) => { res[k] = obj[k]; return res; }, {});
    }

    /**
     * @brief Send raw data through WebSocket connection
     * @param data Data object to send (will be JSON stringified)
     */
    send(data) {
        if (this.isConnected()) {
            console.log("sending:", JSON.stringify(data));
            this.socket.send(JSON.stringify(data));
        }
    }

    /**
     * @brief Set the client name
     * @param name Client identifier string
     */
    setClientName(name) {
        this.clientName = name;
    }
}