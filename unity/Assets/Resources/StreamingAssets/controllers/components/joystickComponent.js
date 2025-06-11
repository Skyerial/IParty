import { Controller } from "../controller.js";

export class JoystickComponent extends Controller {

    constructor(container, vertical = false) {
        super("./views/components/joystickComponentView.html", container, "analogInput" );

        this.vertical = vertical;
        this.origin = { x: 0, y: 0 };
        this.offset = { x: 0, y: 0 };
    }

    bindEvents() {
        const joystick = this.container.querySelector("#joystick");
        const stick = this.container.querySelector(".stick");

        joystick.addEventListener("touchstart", (event) => {
            const touch = event.touches[0];
            this.origin.x = touch.clientX;
            this.origin.y = touch.clientY;
            this.startSendingAnalog();
        });

        joystick.addEventListener("touchmove", (event) => {
            event.preventDefault();
            const touch = event.touches[0];

            const dx = touch.clientX - this.origin.x;
            const dy = touch.clientY - this.origin.y;

            const clampedX = Math.max(-75, Math.min(75, dx));
            const clampedY = Math.max(-75, Math.min(75, dy));

            stick.style.left = `${75 + clampedX}px`;
            stick.style.top = `${75 + clampedY}px`;

            this.offset = this.normalize(clampedX, clampedY, 75);

            const adjusted = this.applyOrientationRotation(this.offset);

            this.updateAnalogInput(adjusted);
        }, { passive: false });

        joystick.addEventListener("touchend", () => {
            const stick = this.container.querySelector(".stick");
    
            stick.style.left = "75px";
            stick.style.top = "75px";

            this.updateAnalogInput({ x: 0, y: 0 });
            this.sendAnalogInput();
            this.stopSendingAnalog();
        });
    }

    normalize(dx, dy, max) {
        const x = Math.max(-1, Math.min(1, dx / max));
        const y = Math.max(-1, Math.min(1, -dy / max));
        return { x, y };
    }

    applyOrientationRotation(input) {
        let adjustedInput = {
                x: -input.y,
                y: input.x
            };

        if (this.vertical) {
            adjustedInput = {
                x: input.x,
                y: input.y
            };
        }
        return adjustedInput;
    }
}
