import { send, isConnected} from "../connection.js";

export class Controller {
    constructor(htmlFile, container, type) {
        this.htmlFile = htmlFile;
        this.container = container;

        this.type = type;
        this.inputInterval = 10;
        this.inputTimer = null;

        this.currentAnalog = { x: 0, y: 0 };
    }

    updateAnalogInput(partialAnalog) {
        this.currentAnalog = Object.assign({}, this.currentAnalog, partialAnalog);
    }

    updateButtonInput(inputName, state) {
        const input = {
            type: this.type,
            "button": inputName,
            "state": state
            // [inputName]: state
        };

        this.sendInput(input);
    }

    startSendingAnalog() {
        if (this.inputTimer) clearInterval(this.inputTimer);
        this.inputTimer = setInterval(() => {
            if (isConnected()) {
                this.sendAnalogInput();
            }
        }, this.inputInterval);
    }

    stopSendingAnalog() {
        clearInterval(this.inputTimer);
        this.inputTimer = null;
    }

    sendAnalogInput() {
        this.sendInput({
            type: this.type,
            x: this.currentAnalog.x,
            y: this.currentAnalog.y
        });
    }

    sendAllInput() {
        
    }

    sendInput(input) {
        if (isConnected()) {
            send(input);
        }
    }

    async init() {
        const html = await this.loadHTML(this.htmlFile);
        this.container.innerHTML = html;
        this.bindEvents();
    }

    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.error(`Failed to load ${file}`);
        }
        return await response.text();
    }

    getContainer() {
        return this.container;
    }

    bindEvents() {}
}
