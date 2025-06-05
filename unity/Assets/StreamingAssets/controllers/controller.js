import { send, isConnected} from "../connection.js";

export class Controller {
    constructor(htmlFile, container) {
        this.htmlFile = htmlFile;
        this.container = container;

        this.inputInterval = 10;
        this.inputTimer = null;

        this.currentAnalog = { x: 0, y: 0 };
        this.previousButtons = {};

        this.init();
    }

    updateAnalogInput(partialAnalog) {
        this.currentAnalog = Object.assign({}, this.currentAnalog, partialAnalog);
    }

    updateButtonInput(newButtons) {
        for (const key in newButtons) {
            if (this.previousButtons[key] !== newButtons[key]) {
                this.previousButtons[key] = newButtons[key];

                const input = {
                    type: "control",
                    x: this.currentAnalog.x,
                    y: this.currentAnalog.y
                };

                input[key] = newButtons[key];

                this.sendInput(input);
            }
        }
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
            type: "control",
            x: this.currentAnalog.x,
            y: this.currentAnalog.y
        });
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

    bindEvents() {}
}
