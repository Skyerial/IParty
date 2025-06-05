import { Controller } from "./controller.js";

export class JoystickController extends Controller {
    constructor(container) {
        super("../views/joystickView.html", container);
    }

    bindEvents() {
        const joystick = this.container.querySelector("#joystick");
        const stick = this.container.querySelector("#stick");

        const btnA = this.container.querySelector(".btn-a");
        const btnB = this.container.querySelector(".btn-b");
        const btnX = this.container.querySelector(".btn-x");
        const btnY = this.container.querySelector(".btn-y");

        let origin = { x: 0, y: 0 };

        joystick.addEventListener("touchstart", this.onTouchStart.bind(this, joystick, stick, origin));
        joystick.addEventListener("touchmove", this.onTouchMove.bind(this, stick, origin), { passive: false });
        joystick.addEventListener("touchend", this.onTouchEnd.bind(this, stick));

        this.bindButton(btnA, "a");
        this.bindButton(btnB, "b");
        this.bindButton(btnX, "x");
        this.bindButton(btnY, "y");
    }

    onTouchStart(joystick, stick, origin, event) {
        const touch = event.touches[0];
        origin.x = touch.clientX;
        origin.y = touch.clientY;
        this.startSendingAnalog();
    }

    onTouchMove(stick, origin, event) {
        event.preventDefault();
        const touch = event.touches[0];

        const dx = touch.clientX - origin.x;
        const dy = touch.clientY - origin.y;
        const clampedX = Math.max(-75, Math.min(75, dx));
        const clampedY = Math.max(-75, Math.min(75, dy));

        stick.style.left = (75 + clampedX) + "px";
        stick.style.top = (75 + clampedY) + "px";

        const original = this.normalize(clampedX, clampedY, 75);

        const rotated = {
        x: -original.y,
        y: original.x
        };

        this.updateAnalogInput(rotated);
    }

    onTouchEnd(stick) {
        this.stopSendingAnalog();
        this.updateAnalogInput({ x: 0, y: 0 });
        this.sendAnalogInput();
        stick.style.left = "75px";
        stick.style.top = "75px";
    }

    normalize(dx, dy, max) {
        let x = dx / max;
        let y = dy / max * -1;
        if (x > 1) x = 1;
        if (x < -1) x = -1;
        if (y > 1) y = 1;
        if (y < -1) y = -1;
        return { x: x, y: y };
    }

    bindButton(element, key) {
        element.addEventListener("touchstart", () => this.updateButtonInput({ [key]: true }));
        element.addEventListener("touchend", () => this.updateButtonInput({ [key]: false }));
    }
}