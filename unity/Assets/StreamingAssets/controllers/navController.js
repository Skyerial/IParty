import { ConnectController } from "./connectController.js";
import { Controller } from "./controller.js";
import { HomeController } from "./homeController.js";

export class NavController extends Controller {
    constructor(container) {
        super("../views/navView.html", container);
    }

    bindEvents() {
        const viewContainer = document.querySelector(".view-container");
        document.querySelector(".site-logo").addEventListener("click", () => {
            new HomeController(viewContainer);
        });

        document.querySelector(".nav-button").addEventListener("click", () => {
            new ConnectController(viewContainer);
        });
    }
 }