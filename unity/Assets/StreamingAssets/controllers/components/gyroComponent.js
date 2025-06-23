import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class GyroComponent extends ViewRenderer {
    constructor(container) {
        super("./views/components/gyroComponentView.html", container);
    }

    bindEvents() {
        this.motionThreshold = 15;
        this.isFlickDetected = false;

        this.permissionPrompt = this.container.querySelector("#permissionPrompt");
        this.playView = this.container.querySelector("#playView");
        this.enableButton = this.container.querySelector("#enableButton");

        this.requestMotionPermission();
    }

    requestMotionPermission() {
        if (typeof DeviceMotionEvent?.requestPermission === "function") {
            DeviceMotionEvent.requestPermission()
                .then((permissionState) => {
                    if (permissionState === "granted") {
                        this.showPlayView();
                        this.startMotionTracking();
                    } else {
                        this.showPermissionPrompt();
                    }
                })
                .catch(() => this.showPermissionPrompt());
        } else {
            this.showPlayView();
            this.startMotionTracking();
        }
    }

    showPlayView() {
        this.permissionPrompt.style.display = "none";
        this.playView.style.display = "block";
    }

    showPermissionPrompt() {
        this.permissionPrompt.style.display = "block";
        this.playView.style.display = "none";
        this.enableButton.addEventListener("click", () => {
            this.requestMotionPermission();
        }, { once: true });
    }

    startMotionTracking() {
        window.addEventListener("devicemotion", this.handleMotionEvent.bind(this), true);
    }

    handleMotionEvent(e) {
        const { acceleration } = e;

        if (
            acceleration?.x > this.motionThreshold ||
            acceleration?.y > this.motionThreshold ||
            acceleration?.z > this.motionThreshold
        ) {
            if (!this.isFlickDetected) {
                this.isFlickDetected = true;
                this.jumpIndicator.style.display = "block";
                socketManager.updateButton("A", true);
                socketManager.updateButton("A", false);
            }
        } else {
            this.isFlickDetected = false;
            this.jumpIndicator.style.display = "none";
        }
    }
}