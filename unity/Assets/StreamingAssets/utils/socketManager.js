import { DpadController } from "../controllers/dpadController.js";
import { JoystickController } from "../controllers/joystickController.js";

export class SocketManager {
    constructor(relayHost = '178.128.247.108', movementType = 'analog') {
        this.socket = null;
        this.relayHost = relayHost;
        this.interval = 10;
        this.timer = null;
        this.isActive = false;

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
        // const isLocal = location.hostname === 'localhost' || location.hostname.startsWith('192.168.');
        const url = !isRemote
            ? `ws://${location.hostname}:8181`
            : `ws://${this.relayHost}:5000/host/${code}/ws`;
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
            // if (this.onMessage) this.onMessage(data); 
        };
        this.socket.onclose = (e) => { 
            console.log('🔴 Disconnected'); 
            console.log(e.code);
            console.log(e.reason);
            console.log(e.wasClean); 
            this.socket = null; 
            // if (this.onClose) this.onClose(); 
        };
    }

    loadController(controller) {
        let root = document.querySelector(".view-container");

        if (controller == "dpad-preset") {
            let js = new DpadController(root)
            js.init()
        } else if (controller == "joystick-preset") {
            let js = new JoystickController(root)
            js.init()
        } else if (controller == "one-button") {
            let js = new OneButton(root)
            js.init()
        }
    }

    handleCommand(data) {
          if (data.type == "controller") {
                this.loadController(data.controller)
            }
            console.log(data.type);
            console.log(data.controller);
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

    getFilteredState() {
        const base = (({ type, A, B, C, D, button }) => ({ type, A, B, C, D, button }))(this.state);
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
        if (this.socket.readyState === WebSocket.OPEN) {
            console.log("sending:", JSON.stringify(data));
            this.socket.send(JSON.stringify(data));
        }
    }
}
