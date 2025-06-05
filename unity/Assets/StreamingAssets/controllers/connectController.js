import { Controller } from "./controller.js"
import { PlaceHolderController } from "./placeHolderController.js";

export class ConnectController extends Controller {
    constructor(container) {
        super("../views/connectView.html", container);
    }

    bindEvents() {
        let inputfield = document.querySelector(".lobby-input");
        document.querySelector(".lobby-button").addEventListener("click", () => {
            this.connect(inputfield.value);
        });
    }

    connect(code) {
        const ws = new WebSocket(`ws://${location.hostname}:${code}`);
    }
}