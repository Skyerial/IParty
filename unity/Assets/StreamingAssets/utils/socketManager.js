import { DpadController } from "../controllers/dpadController.js";
import { GyroController } from "../controllers/gyroController.js";
import { HotPotatoController } from "../controllers/hotPotatoController.js";
import { JoystickController } from "../controllers/joystickController.js";
import { TextController } from "../controllers/textController.js";
import { OneButton } from "../controllers/oneButton.js";
import { ReadyController } from "../controllers/readyController.js";
import { CustomJoystickController } from "../controllers/customJoystickController.js";
import { TankJoystickController } from "../controllers/tankJoystickController.js";
import { initializeController, loadPage } from "../main.js";
import { getController } from "../main.js";
import { DescriptionController } from "../controllers/descriptionController.js";
import { WaitingPage } from "../pages/waitingPage.js";

/**
 * @brief Mapping of controller type identifiers to their corresponding controller classes.
 * @details Used for dynamic instantiation based on the 'controller' field from server messages.
 */
const CONTROLLER_MAP = {
  "dpad-preset": DpadController,
  "joystick-preset": JoystickController,
  "one-button": OneButton,
  "turf": CustomJoystickController,
  "spleef": CustomJoystickController,
  "ready": ReadyController,
  "tank": TankJoystickController,
  "description": DescriptionController
};

/**
 * @brief Manages a WebSocket connection for sending and receiving controller data.
 * @details Handles connection setup, message parsing, and updates for different input types.
 */
export class SocketManager {
  /**
   * @brief Constructs a SocketManager instance.
   * @param {string} relayHost - Hostname of the relay server. Defaults to 'iparty.duckdns.org'.
   * @param {string} movementType - Initial movement mode ('analog' or 'dpad'). Defaults to 'analog'.
   */
  constructor(relayHost = "iparty.duckdns.org", movementType = "analog") {
    this.socket = null;
    this.relayHost = relayHost;
    this.interval = 10;
    this.timer = null;
    this.isActive = false;

    this.clientName = null;

    this.onOpen = null;
    this.onMessage = null;
    this.onClose = null;

    this.lastSentMessage = null;
    this.changeMovementType(movementType);
  }

  /**
   * @brief Sets the active movement type and initializes state fields.
   * @param {string} type - 'analog' for continuous input or 'dpad' for directional input.
   */
  changeMovementType(type) {
    this.activeMovementType = type === "dpad" ? "dpad" : "analog";
    this.state = {
      type,
      A: false,
      B: false,
      C: false,
      D: false,
      button: false,
      T: "",
    };

    if (this.activeMovementType === "analog") {
      Object.assign(this.state, { x: 0, y: 0 });
    } else {
      Object.assign(this.state, {
        up: false,
        down: false,
        left: false,
        right: false,
      });
    }
  }

  /**
   * @brief Opens a WebSocket connection using the provided room code.
   * @param {string} code - Room code for host connections.
   */
  connect(code) {
    if (this.socket) return;
    const isRemote = location.hostname === this.relayHost;
    const url = !isRemote
      ? `wss://${location.hostname}:8181`
      : `wss://${this.relayHost}:5001/host/${code}/ws`;

    console.log(url);

    this.socket = new WebSocket(url);
    this.socket.onopen = () => console.log("Connected");

    this.socket.onmessage = async (e) => {
      let raw = e.data instanceof Blob ? await e.data.text() : e.data;
      const data = JSON.parse(raw);
      this.handleCommand(data);
    };

    this.socket.onclose = (e) => {
      console.log("Disconnected", e.code, e.reason);
      this.socket = null;
    };
  }

  /**
   * @brief Loads and initializes the appropriate controller view based on incoming data.
   * @param {object} data - Parsed message data, including 'controller' and optional 'playerstats'.
   */
  loadController(data) {
    const root = document.querySelector(".view-container");
    const { controller, playerstats = [] } = data;

    if (controller === "hotpotato") {
      const list = playerstats.map(({ name, color, button }) => ({
        itemName: name,
        itemColor: color,
        connectedBtn: button,
      }));
      initializeController(HotPotatoController, root, list);
    } else if (controller == "text-preset") {
      initializeController(TextController, root, data.words);
    } else if (controller == "whackamole") {
      initializeController(GyroController, root, "Shake your phone to whack a mole");
    } else if (controller == "mainboard") {
      initializeController(GyroController, root, "Shake your phone to throw the dice");
    } else if (controller == "waitingpage") {
      loadPage(WaitingPage, root);
    } else if (CONTROLLER_MAP[controller]) {
      initializeController(CONTROLLER_MAP[controller], root);
    } else {
      console.warn(`Unknown controller type: ${controller}`);
    }
  }

  /**
   * @brief Handles death updates for the HotPotatoController.
   * @param {object} data - The data payload containing playerName.
   */
  handleDeadUpdate(data) {
    const hController = getController();
    if (hController instanceof HotPotatoController) {
      hController.removePlayer(data.playerName);
    }
  }

  /**
   * @brief Processes incoming command data and updates state or UI accordingly.
   * @param {object} data - Parsed message data with a 'type' field.
   */
  handleCommand(data) {
    const root = document.querySelector(".view-container");
    switch (data.type) {
      case "ping":
        const pong = JSON.stringify({ type: "pong" });
        this.send(pong);
        break;
      case "controller":
        this.loadController(data);
        break;
      case "reconnect-status":
        data.approved
          ? this.loadController(data)
          : alert("Player not found, check the code.");
        break;
      case "character-status":
        if (data.approved) {
          this.setClientName(data.name);
          initializeController(JoystickController, root);
        } else {
          alert("Color taken, choose another.");
        }
        break;
      case "hp-deadupdate":
        this.handleDeadUpdate(data);
        break;
    }
    console.log(data);
  }

  /**
   * @brief Closes the WebSocket connection if it is open.
   */
  disconnect() {
    if (this.isConnected()) this.socket.close();
  }

  /**
   * @brief Checks if the WebSocket is currently open.
   * @returns {boolean} True if connected; false otherwise.
   */
  isConnected() {
    return this.socket?.readyState === WebSocket.OPEN;
  }

  /**
   * @brief Registers custom handlers for open, message, and close events.
   * @param {object} handlers - Object containing onOpen, onMessage, onClose callbacks.
   */
  setHandlers({ onOpen, onMessage, onClose }) {
    this.onOpen = onOpen;
    this.onMessage = onMessage;
    this.onClose = onClose;
  }

  /**
   * @brief Updates analog stick coordinates and triggers send loop if needed.
   * @param {number} x - X-axis value between -1 and 1.
   * @param {number} y - Y-axis value between -1 and 1.
   */
  updateAnalog(x, y) {
    if (this.activeMovementType !== "analog") return;
    const changed =
      Math.abs(x - this.state.x) > 0.01 || Math.abs(y - this.state.y) > 0.01;
    if (!changed && (x !== 0 || y !== 0)) return;
    this.state.x = x;
    this.state.y = y;
    this.updateActivity();
  }

  /**
   * @brief Updates D-pad directional state and sends if connected.
   * @param {string} direction - One of 'up', 'down', 'left', 'right'.
   * @param {boolean} value - Pressed (true) or released (false).
   */
  updateDpad(direction, value) {
    if (this.activeMovementType !== "dpad") return;
    this.state[direction] = value;
    if (this.isConnected()) this.sendFiltered();
  }

  /**
   * @brief Updates button press state and sends if connected.
   * @param {string} name - Button identifier (e.g., 'A', 'B').
   * @param {boolean} value - Pressed (true) or released (false).
   */
  updateButton(name, value) {
    this.state[name] = value;
    if (this.isConnected()) this.sendFiltered();
  }

  /**
   * @brief Updates text input state and sends if connected.
   * @param {string} char - The current text value.
   */
  updateText(char) {
    this.state.T = char;
    if (this.isConnected()) this.sendFiltered();
  }

  /**
   * @brief Builds an object with only the relevant state fields for transmission.
   * @returns {object} Filtered state object.
   */
  getFilteredState() {
    const baseKeys = ["type", "A", "B", "C", "D", "button", "T"];
    const base = baseKeys.reduce((o, k) => ((o[k] = this.state[k]), o), {});
    const moveKeys = this.getMovementKeys();
    moveKeys.forEach((k) => (base[k] = this.state[k]));
    return base;
  }

  /**
   * @brief Sends the filtered state if it has changed since last send.
   */
  sendFiltered() {
    if (!this.isConnected()) return;
    const msg = JSON.stringify(this.getFilteredState());
    if (msg === this.lastSentMessage) return;
    this.lastSentMessage = msg;
    console.log(msg);
    this.socket.send(msg);
  }

  /**
   * @brief Starts a periodic send loop for analog updates.
   */
  startLoop() {
    if (this.timer) return;
    this.timer = setInterval(() => {
      if (this.isActive && this.isConnected()) this.sendFiltered();
    }, this.interval);
  }

  /**
   * @brief Stops the periodic send loop.
   */
  stopLoop() {
    clearInterval(this.timer);
    this.timer = null;
  }

  /**
   * @brief Manages analog activity state and send loop based on movement.
   */
  updateActivity() {
    if (this.activeMovementType !== "analog") return;
    const moving = this.state.x !== 0 || this.state.y !== 0;
    if (moving && !this.isActive) {
      this.isActive = true;
      this.startLoop();
    } else if (!moving && this.isActive) {
      this.isActive = false;
      this.stopLoop();
      if (this.isConnected()) this.sendFiltered();
    }
  }

  /**
   * @brief Returns the movement keys appropriate for the current mode.
   * @returns {string[]} Array of movement key names.
   */
  getMovementKeys() {
    return this.activeMovementType === "analog"
      ? ["x", "y"]
      : ["up", "down", "left", "right"];
  }

  /**
   * @brief Utility to pick only specified keys from an object.
   * @param {object} obj - Source object.
   * @param {string[]} keys - Keys to select.
   * @returns {object} Object containing only the selected key-value pairs.
   */
  pick(obj, keys) {
    return keys.reduce((res, k) => ((res[k] = obj[k]), res), {});
  }

  /**
   * @brief Sends arbitrary data over the WebSocket if connected.
   * @param {object} data - Data to send.
   */
  send(data) {
    if (this.isConnected()) {
      const msg = JSON.stringify(data);
      console.log("sending:", msg);
      this.socket.send(msg);
    }
  }

  /**
   * @brief Stores the client name for identification.
   * @param {string} name - Name to set as client identifier.
   */
  setClientName(name) {
    this.clientName = name;
  }
}
