import { JoystickController } from "../controllers/joystickController.js";
import { ViewRenderer } from "../utils/viewRenderer.js";
import { socketManager } from "../main.js";
import { Login } from "../login/login.js";
import { ListComponent } from "../controllers/components/listComponent.js";
import { HotPotatoController } from "../controllers/hotPotatoController.js";
import { OneButton } from "../controllers/oneButton.js";
import { Reconnect } from "../reconnect/reconnect.js";


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
            const js = new Login(this.container)
            await js.init()

            // Init joystick
            // const js = new JoystickController(this.container);
            // await js.init();
        });

        document.querySelector(".lobby-reconnect").addEventListener("click", async () => {
            if (!socketManager.isConnected()) {
                socketManager.connect(inputfield.value);
            }

            const js = new Reconnect(this.container)
            await js.init()
        });
    }
}