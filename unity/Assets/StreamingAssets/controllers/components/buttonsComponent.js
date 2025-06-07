import { Controller } from "../controller.js";

export class ButtonsComponent extends Controller {
    constructor(container, vertical = false) {
        super("../views/components/buttonsComponentView.html", container, "buttonInput");
        this.vertical = vertical;
    }

    bindEvents() {
        if (this.vertical) {
            let container = this.container.querySelector(".button-container");
            container.classList.add("orientation-vertical");
        }

        let buttons = this.container.querySelectorAll(".game-button");

        for (const button of buttons) {
            button.addEventListener("touchstart", () => {
                let buttonText = button.innerHTML.toLowerCase();
                this.updateButtonInput(buttonText, true);
            });

            button.addEventListener("touchend", () => {
                let buttonText = button.innerHTML.toLowerCase();
                this.updateButtonInput(buttonText, false);
            });
        }
    }
}