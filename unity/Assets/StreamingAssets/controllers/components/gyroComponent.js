import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class GyroComponent extends ViewRenderer {
    constructor(container) {
        super("./views/components/gyroComponentView.html", container);
    }

    bindEvents() {
        const motionThreshold = 15; // example threshold
        let isFlickDetected = false;
        const enableButton = this.container.querySelector("#enableButton");
        const jumpIndicator = this.container.querySelector("#jumpIndicator");
        const gyroDataDisplay = this.container.querySelector("#gyroData");

        function handleMotionEvent(e) {
            const acceleration = e.acceleration;
            const rotation = e.rotationRate;

            // Update gyro data display
            if (rotation) {
                gyroDataDisplay.innerHTML = `
                <strong>Gyroscope:</strong><br>
                Alpha (Z): ${rotation.alpha?.toFixed(2) || "0"} °/s<br>
                Beta (X): ${rotation.beta?.toFixed(2) || "0"} °/s<br>
                Gamma (Y): ${rotation.gamma?.toFixed(2) || "0"} °/s
            `;
            }

            // Check motion threshold
            if (
                acceleration?.x > motionThreshold ||
                acceleration?.y > motionThreshold ||
                acceleration?.z > motionThreshold
            ) {
                if (!isFlickDetected) {
                    isFlickDetected = true;
                    jumpIndicator.style.display = "block";
                    socketManager.updateButton("A", true);
                    socketManager.updateButton("A", false);
                }
            } else {
                isFlickDetected = false;
                jumpIndicator.style.display = "none";
            }
        }

        enableButton.addEventListener('click', () => {
            if (typeof DeviceMotionEvent.requestPermission === "function") {
                DeviceMotionEvent.requestPermission().then((permissionState) => {
                    if (permissionState === "granted") {
                        hideEnableButton();
                        window.addEventListener("devicemotion", handleMotionEvent, true);
                    }
                });
            } else {
                hideEnableButton();
                window.addEventListener("devicemotion", handleMotionEvent, true);
            }
        })

        function hideEnableButton() {
            document.getElementById("enableButton").style.display = "none";
        }
    }
}