import { Controller } from "./controller.js"
import { JoystickController } from "./joystickController.js";
import { isConnected, connectToServer } from "../connection.js";

export class ConnectPage extends Controller {
    constructor(container) {
        super("../views/connectView.html", container);
    }

    bindEvents() {
        let inputfield = document.querySelector(".lobby-input");
        document.querySelector(".lobby-button").addEventListener("click", async () => {
            if (!isConnected()) {
                connectToServer(inputfield.value);
            }

            const js = new JoystickController(this.container);
            await js.init();
        });
    }
}