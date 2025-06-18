import { socketManager } from "../main.js";
import { JoystickController } from "../controllers/joystickController.js";
import { color_change } from "./background_render.js";
import { OneButton } from "../controllers/oneButton.js";

function main() {
    let root = document.querySelector(".view-container");
    const colorBoxes = document.querySelectorAll('.color-box');

    colorBoxes.forEach(box => {
    box.addEventListener('touchstart', () => {
        box.classList.add('active');
    });

    box.addEventListener('touchend', () => {
        box.classList.remove('active');
    });
    });

    const playButton = document.querySelector('#play-button');
    const nameInput = document.getElementById('user');

    let selectedColor = null;

    // Track selected color
    colorBoxes.forEach(box => {
    box.addEventListener('click', () => {
        // Remove previous selection
        colorBoxes.forEach(b => b.classList.remove('selected'));

        // Add new selection
        box.classList.add('selected');
        selectedColor = box.id; // Save selected color ID
        color_change(selectedColor);
    });
    });

    // Handle play button click
    playButton.addEventListener('click', () => {

    // Handle player name by trimming of whitespaces
    const playerName = nameInput.value.trim();

    // If no name is selected call an alert
    if (!playerName) {
        alert('Please enter a player name.');
        return;
    }

    // If no color is selected call an alert.
    if (!selectedColor) {
        alert('Please select a color.');
        return;
    }

    // Build JSON object
    const playerData = {
        name: playerName,
        color: selectedColor
    };


    // NEW CHARACTER CREATION
    socketManager.send(playerData)
    const js = new JoystickController(root);
    js.init();
    });
}
main();