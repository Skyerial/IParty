import { socketManager } from "../main.js";
import { ViewRenderer } from "../utils/viewRenderer.js";

export class ReconnectPage extends ViewRenderer {
    constructor(container) {
        super("./views/reconnectView.html", container);
        this.container = container
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
