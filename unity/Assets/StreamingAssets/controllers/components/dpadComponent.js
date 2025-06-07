import { Controller } from "../controller.js";

export class DpadComponent extends Controller {
    constructor(container, vertical) {
        super("../views/components/dpadComponentView.html", container, "dpadInput");
        this.vertical = vertical;
    }

    bindEvents () {
        if (this.vertical) {
            let container = this.container.querySelector(".dpad-container");
            container.classList.add("orientation-vertical");
        }

        this.bindButton(this.container.querySelector(".dpad-up"), "up")
        this.bindButton(this.container.querySelector(".dpad-down"), "down")
        this.bindButton(this.container.querySelector(".dpad-right"), "right")
        this.bindButton(this.container.querySelector(".dpad-left"), "left")
    }

    bindButton(button, inputName) {
        console.log(inputName);
        button.addEventListener("touchstart", () => {
            this.updateButtonInput(inputName, true);
        });

        button.addEventListener("touchend", () => {
            this.updateButtonInput(inputName, false);
        });
    }
}