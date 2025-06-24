import { Controller } from "./controller.js";

export class NameController extends Controller {
    constructor(container, socket) {
        super("./views/userView.html", container, socket);
    }

    bindEvents() {
        const nameInput = document.querySelector(".name-input");
        const confirmButton = document.querySelector(".lobby-button");

        confirmButton.addEventListener("click", () => {
            let rawName = nameInput.value;
            let trimmedName = rawName.trim();
            const maxLength = 20;

            // Apply limit
            let finalName = trimmedName.substring(0, maxLength);

            if (finalName.length === 0) {
                alert("Name cannot be empty.");
                return;
            }

            console.log("Final trimmed name:", finalName);
            // You can now store it, send it, or switch views
            // Example: move to lobby screen
            // new LobbyController(this.container, this.socket, finalName);
        });
    }
}
