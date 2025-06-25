import { DpadController } from "../controllers/dpadController.js";
import { GyroController } from "../controllers/gyroController.js";
import { HotPotatoController } from "../controllers/hotPotatoController.js";
import { JoystickController } from "../controllers/joystickController.js";
import { TextController } from "../controllers/textController.js";
import { OneButton } from "../controllers/oneButton.js";
import { ReadyController } from "../controllers/readyController.js";
import { CustomJoystickController } from "../controllers/customJoystickController.js";
import { TankJoystickController } from "../controllers/tankJoystickController.js";

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
  "gyro": GyroController,
  "tank": TankJoystickController
};

/**
 * SocketManager
 *
 * Manages a WebSocket connection for sending and receiving controller data.
 * Handles connection setup, message parsing, and state updates for different input types.
 */
export class SocketManager {
  /**
   * Creates a new SocketManager.
   *
   * @param {string} relayHost - Hostname for the relay server (default: 'iparty.duckdns.org').
   * @param {string} movementType - Initial movement mode ('analog' or 'dpad').
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
   * Sets movement mode and resets state fields.
   * Supports 'analog' or 'dpad' modes.
   *
   * @param {string} type - 'analog' for continuous input, 'dpad' for directional input.
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
   * Opens a WebSocket connection using the provided room code.
   *
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
   * Chooses and initializes the correct controller based on incoming data.
   *
   * @param {object} data - Parsed message data, including controller type and stats.
   */
  loadController(data) {
    const root = document.querySelector(".view-container");
    const { controller, playerstats = [] } = data;

    if (controller === "hotpotato") {
      const list = playerstats.map(({ name, color, button }) => ({
        label: name,
        color,
        jsonButton: button,
      }));

      new HotPotatoController(root, list).init();
    } else if (CONTROLLER_MAP[controller]) {
      new CONTROLLER_MAP[controller](root).init();
    } else {
      console.warn(`Unknown controller type: ${controller}`);
    }
  }

  /**
   * Processes incoming commands and updates UI or state accordingly.
   *
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
      case "clear-text":
        document.querySelector("#myText").value = "";
        break;
      case "reconnect-status":
        data.approved
          ? new JoystickController(root).init()
          : alert("Player not found, check the code.");
        break;
      case "character-status":
        if (data.approved) {
          this.setClientName(data.name);
          new JoystickController(root).init();
        } else {
          alert("Color taken, choose another.");
        }
        break;
    }
    console.log(data);
  }

  /**
   * Closes the WebSocket if it is open.
   */
  disconnect() {
    if (this.isConnected()) this.socket.close();
  }

  /**
   * Checks if the WebSocket is currently open.
   *
   * @returns {boolean} True if connected, false otherwise.
   */
  isConnected() {
    return this.socket?.readyState === WebSocket.OPEN;
  }

  /**
   * Saves custom handlers for open, message, and close events.
   *
   * @param {object} handlers - Object with onOpen, onMessage, onClose functions.
   */
  setHandlers({ onOpen, onMessage, onClose }) {
    this.onOpen = onOpen;
    this.onMessage = onMessage;
    this.onClose = onClose;
  }

  /**
   * Updates analog stick coordinates and triggers sending if needed.
   *
   * @param {number} x - X axis value between -1 and 1.
   * @param {number} y - Y axis value between -1 and 1.
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
   * Updates a D-pad direction state and sends if connected.
   *
   * @param {string} direction - 'up', 'down', 'left', or 'right'.
   * @param {boolean} value - Pressed (true) or released (false).
   */
  updateDpad(direction, value) {
    if (this.activeMovementType !== "dpad") return;
    this.state[direction] = value;
    if (this.isConnected()) this.sendFiltered();
  }

  /**
   * Updates a button press state and sends if connected.
   *
   * @param {string} name - Button identifier (e.g., 'A', 'B').
   * @param {boolean} value - Pressed (true) or released (false).
   */
  updateButton(name, value) {
    this.state[name] = value;
    if (this.isConnected()) this.sendFiltered();
  }

  /**
   * Updates text input state and sends if connected.
   *
   * @param {string} char - New text character.
   */
  updateText(char) {
    this.state.T = char;
    if (this.isConnected()) this.sendFiltered();
  }

  /**
   * Builds an object with only the relevant state fields.
   *
   * @returns {object} Filtered state ready for sending.
   */
  getFilteredState() {
    const baseKeys = ["type", "A", "B", "C", "D", "button", "T"];
    const base = baseKeys.reduce((o, k) => ((o[k] = this.state[k]), o), {});
    const moveKeys = this.getMovementKeys();
    moveKeys.forEach((k) => (base[k] = this.state[k]));
    return base;
  }

  /**
   * Sends the filtered state if it has changed.
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
   * Starts a loop to send analog updates at intervals when active.
   */
  startLoop() {
    if (this.timer) return;
    this.timer = setInterval(() => {
      if (this.isActive && this.isConnected()) this.sendFiltered();
    }, this.interval);
  }

  /**
   * Stops the analog update loop.
   */
  stopLoop() {
    clearInterval(this.timer);
    this.timer = null;
  }

  /**
   * Tracks analog stick activity and manages the send loop.
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
   * Returns the movement keys for the current mode.
   *
   * @returns {string[]} Array of movement key names.
   */
  getMovementKeys() {
    return this.activeMovementType === "analog"
      ? ["x", "y"]
      : ["up", "down", "left", "right"];
  }

  /**
   * Picks only specified keys from an object.
   *
   * @param {object} obj - Source object.
   * @param {string[]} keys - Keys to pick.
   * @returns {object} Object with only the selected keys.
   */
  pick(obj, keys) {
    return keys.reduce((res, k) => ((res[k] = obj[k]), res), {});
  }

  /**
   * Sends arbitrary data over the WebSocket.
   *
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
   * Stores the client name.
   *
   * @param {string} name - Name to set as client identifier.
   */
  setClientName(name) {
    this.clientName = name;
  }
}
