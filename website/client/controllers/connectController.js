import { Controller } from "./controller.js"
import { PlaceHolderController } from "./placeHolderController.js";

export class ConnectController extends Controller {
    constructor(container) {
        super("../views/connectView.html", container);
    }

    bindEvents() {
        document.querySelector(".lobby-button").addEventListener("click", () => {
            new PlaceHolderController(this.container, null);
        });    
    }
}