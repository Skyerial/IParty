import { Controller } from "./controller.js";
import { HomeController } from "./homeController.js";

export class PlaceHolderController extends Controller {
    constructor (container, socket) {
        super("../views/placeHolderView.html", container);
    }

    // Main logic of controller here
    bindEvents() {

        // For example, go back to the homepage
        document.querySelector("#homebtn").addEventListener("click", () => {
            new HomeController(this.container, null);
        });

        // Or maybe send input to the unity program
        document.querySelector("#left").addEventListener("click", () => {
            this.sendInput("go_left");
        });

        document.querySelector("#right").addEventListener("click", () => {
            this.sendInput("go_right");
        });
    }
}