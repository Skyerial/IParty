import { socketManager } from "../main.js";
import { JoystickController } from "../controllers/joystickController.js";

export class Reconnect {
    constructor(container) {
        this.container = container
    }

    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.error(`Failed to load ${file}`);
        }
        return await response.text();
    }

    async init() {
        const html = await this.loadHTML("./reconnect/reconnect.html");
        this.container.innerHTML = html;
        this.bindevents();
    }
    
    bindevents() {
        let inputfield = document.querySelector(".reconnect-input");
        document.querySelector(".reconnect").addEventListener("click", async () => {
            if (inputfield.value.trim() === "") {
                alert("Input field is empty!");
            }

            const data = { type: "reconnect", code: inputfield.value }
            socketManager.send(data)
        });
    }
}
