import { ConnectController } from "./connectController.js";
import { Controller } from "./controller.js";
import { HomeController } from "./homeController.js";
import { isConnecting, disconnect, isConnected } from "../connection.js";

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
            if (isConnected()) {
                disconnect();
                new HomeController(viewContainer);
            } else {
                new ConnectController(viewContainer);
            }

            this.updateNavItem();
        });
    }

    updateNavItem() {
        let button = document.querySelector(".nav-button");
        if (isConnected() || isConnecting()) {
            button.innerHTML = "Disconnect";
        } else {
            button.innerHTML = "Connect";
        }

        console.log("Hey");
    }
}