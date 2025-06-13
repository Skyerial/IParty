import { Controller } from "./controller.js"
import { JoystickController } from "./joystickController.js";
import { isConnected, connectToServer } from "../connection.js";
import { Login } from "../login/login.js";

export class ConnectPage extends Controller {
    constructor(container, created) {
        super("./views/connectView.html", container);
        this.created = created;
    }

    bindEvents() {
        let inputfield = document.querySelector(".lobby-input");
        document.querySelector(".lobby-button").addEventListener("click", async () => {
            if (!isConnected()) {
                connectToServer(inputfield.value);
            }
                
            // TEST: Init character login
            const js = new Login(this.container)
            await js.init()

            // Init joystick 
            // const js = new JoystickController(this.container);
            // await js.init();
        });
    }
}