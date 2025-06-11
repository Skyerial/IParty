import { Controller } from "../controller.js";

export class ButtonComponent extends Controller {
    constructor(container, vertical = false) {
        super("./views/components/buttonComponentView.html", container, "buttonInput");
    }

    bindEvents() {
        let button = this.container.querySelector(".big-button");

        button.addEventListener("touchstart", () => {
            this.updateButtonInput("button", true);
        });

        button.addEventListener("touchend", () => {
            this.updateButtonInput("button", false);
        });
    }
} 