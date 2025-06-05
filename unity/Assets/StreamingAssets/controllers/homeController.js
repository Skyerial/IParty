import { Controller } from "./controller.js";
import { PlaceHolderController } from "./placeHolderController.js";

export class HomeController extends Controller {
    constructor (container) {
        super("../views/homeView.html", container);
    }


    bindEvents() {
        let placeh = this.container.querySelector("#placeholder");

        placeh.addEventListener("click", () => {
            let placeholder = new PlaceHolderController(this.container, null);
        });
    }
}