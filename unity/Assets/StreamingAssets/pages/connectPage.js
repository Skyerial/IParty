import { JoystickController } from "./joystickController.js";
import { ViewRenderer } from "../utils/viewRenderer.js";
import { socketManager } from "../main.js";


export class ConnectPage extends ViewRenderer {
    constructor(container, created) {
        super("./views/connectView.html", container);
        this.created = created;
    }

    bindEvents() {
        let inputfield = document.querySelector(".lobby-input");
        document.querySelector(".lobby-button").addEventListener("click", async () => {
            if (!socketManager.isConnected()) {
                socketManager.connect(inputfield.value);
            }
                
            // TEST: Init character login
            // const js = new Login(this.container)
            // await js.init()

            // Init joystick 
            const js = new JoystickController(this.container);
            await js.init();
        });
    }
}