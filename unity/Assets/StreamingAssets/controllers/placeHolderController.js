import { BaseController } from "./baseController.js";
import { HomeController } from "./homeController.js";

export class PlaceHolderController extends BaseController {
    constructor (container, socket) {
        super("../views/placeHolderView.html", container, socket);
    }

    // Main logic of controller here
    bindEvents() {

        // For example, go back to the homepage
        this.container.querySelector("#homebtn").addEventListener("click", () => {
            new HomeController(this.container, null);
        });

        // Or maybe send input to the unity program
        this.container.querySelector("#left").addEventListener("click", () => {
            this.sendInput("go_left");
        });

        this.container.querySelector("#right").addEventListener("click", () => {
            this.sendInput("go_right");
        });
    }
}