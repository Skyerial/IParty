import { ConnectController } from "./connectController.js";
import { Controller } from "./controller.js";

export class HomeController extends Controller {
    constructor (container) {
        super("../views/homeView.html", container);
    }


    bindEvents() {
        document.querySelector(".home-button").addEventListener("click", () => {
            new ConnectController(this.container);
        });
    }
}