import { DpadController } from "../controllers/dpadController.js";
import { GyroController } from "../controllers/gyroController.js";
import { HotPotatoController } from "../controllers/hotPotatoController.js";
import { JoystickController } from "../controllers/joystickController.js";
import { TextController } from "../controllers/textController.js";
import { OneButton } from "../controllers/oneButton.js";
import { ReadyController } from "../controllers/readyController.js";
import { CustomJoystickController } from "../controllers/customJoystickController.js";

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

export class SocketManager {
    constructor(relayHost = 'iparty.duckdns.org', movementType = 'analog') {
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

    // Switch between 'analog' and 'dpad', resetting state shape
    changeMovementType(type) {
        this.activeMovementType = type === 'dpad' ? 'dpad' : 'analog';
        // base controller buttons
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
        }
        console.log(JSON.stringify(data));
    }

    disconnect() {
        if (this.isConnected()) this.socket.close();
    }

    isConnected() {
        return this.socket?.readyState === WebSocket.OPEN;
    }

    setHandlers({ onOpen, onMessage, onClose }) {
        this.onOpen = onOpen;
        this.onMessage = onMessage;
        this.onClose = onClose;
    }

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

    updateDpad(direction, value) {
        if (this.activeMovementType !== 'dpad') return;

        this.state[direction] = value;
        if (this.isConnected()) {
            this.sendFiltered();
        }
    }

    updateButton(name, value) {
        this.state[name] = value;
        if (this.isConnected()) {
            this.sendFiltered();
        }
    }

    updateText(char) {
        this.state.T = char
        if (this.isConnected()) {
            this.sendFiltered();
        }
    }

    getFilteredState() {
        const base = (({ type, A, B, C, D, button, T }) => ({ type, A, B, C, D, button, T }))(this.state);
        const movement = this.pick(this.state, this.getMovementKeys());
        return { ...base, ...movement };
    }

    sendFiltered() {
        if (!this.isConnected()) return;
        const message = JSON.stringify(this.getFilteredState());
        if (message === this.lastSentMessage) return; // Don't send duplicates
        this.lastSentMessage = message;
        console.log(message);
        this.socket.send(message);
    }

    startLoop() {
        if (this.timer) return;
        this.timer = setInterval(() => {
            if (this.isActive && this.isConnected()) {
                this.sendFiltered();
            }
        }, this.interval);
    }

    stopLoop() {
        clearInterval(this.timer);
        this.timer = null;
    }

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

    getMovementKeys() {
        return this.activeMovementType === 'analog' ? ['x', 'y'] : ['up', 'down', 'left', 'right'];
    }

    pick(obj, keys) {
        return keys.reduce((res, k) => { res[k] = obj[k]; return res; }, {});
    }

    send(data) {
        if (this.isConnected()) {
            console.log("sending:", JSON.stringify(data));
            this.socket.send(JSON.stringify(data));
        }
    }

    setClientName(name) {
        this.clientName = name;
    }
}
