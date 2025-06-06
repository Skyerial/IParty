import { Controller } from "./controller.js"
import { connectToServer, isConnected } from "../connection.js";
import { JoystickController } from "./joystickController.js";
import { nav } from "../connection.js";

export class ConnectController extends Controller {
    constructor(container) {
        super("../views/connectView.html", container);
    }

    bindEvents() {
        let inputfield = document.querySelector(".lobby-input");
        document.querySelector(".lobby-button").addEventListener("click", () => {
            if (!isConnected()) {
                connectToServer(inputfield.value);
            }

            new JoystickController(this.container);
            nav.updateNavItem("Disconnect");
        });
    }
}