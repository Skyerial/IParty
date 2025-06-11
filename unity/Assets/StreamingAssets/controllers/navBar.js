import { ConnectPage } from "./connectPage.js";
import { Controller } from "./controller.js";
import { isConnected } from "../connection.js";

export class NavBar extends Controller {
    constructor(container) {
        super("./views/navView.html", container);
    }

    bindEvents() {
        const viewContainer = document.querySelector(".view-container");

        document.querySelector(".site-logo").addEventListener("click", () => {
            if (!isConnected()) {
                new ConnectPage(viewContainer);
            }
        });
    }
}