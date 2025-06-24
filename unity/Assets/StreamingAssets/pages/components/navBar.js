import { ConnectPage } from "../connectPage.js";
import { ViewRenderer } from "../../utils/viewRenderer.js";
import { socketManager } from "../../main.js";

export class NavBar extends ViewRenderer {
    constructor(container) {
        super("./views/navView.html", container);
    }

    bindEvents() {
        const viewContainer = document.querySelector(".view-container");

        document.querySelector(".site-logo").addEventListener("click", () => {
            if (!socketManager.isConnected()) {
                new ConnectPage(viewContainer);
            }
        });
    }
}