import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

/**
 * GyroComponent
 *
 * Manages device motion input by requesting permission, showing prompts,
 * and detecting flick gestures to send button press events.
 */
export class GyroComponent extends ViewRenderer {
  /**
   * Create a GyroComponent
   * 
   * @param {HTMLElement} container - Element where the gyro view will be rendered.
   */
  constructor(container) {
    super("./views/components/gyroComponentView.html", container);
  }

  /**
   * Sets up motion threshold, UI elements, and starts permission flow.
   */
  bindEvents() {
    this.motionThreshold = 15;
    this.isFlickDetected = false;

    this.permissionPrompt = this.container.querySelector("#permissionPrompt");
    this.playView = this.container.querySelector("#playView");
    this.enableButton = this.container.querySelector("#enableButton");

    this.requestMotionPermission();
  }

  /**
   * Requests permission for device motion events if needed,
   * then shows the play view and starts tracking or shows the prompt.
   */
  requestMotionPermission() {
    if (typeof DeviceMotionEvent?.requestPermission === "function") {
      DeviceMotionEvent.requestPermission()
        .then(permissionState => {
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

  /**
   * Displays the main play UI, hiding the permission prompt.
   */
  showPlayView() {
    this.permissionPrompt.style.display = "none";
    this.playView.style.display = "block";
  }

  /**
   * Displays the permission prompt and sets up the enable button listener.
   */
  showPermissionPrompt() {
    this.permissionPrompt.style.display = "block";
    this.playView.style.display = "none";
    this.enableButton.addEventListener(
      "click",
      () => this.requestMotionPermission(),
      { once: true }
    );
  }

  /**
   * Starts listening to device motion events.
   */
  startMotionTracking() {
    window.addEventListener(
      "devicemotion",
      this.handleMotionEvent.bind(this),
      true
    );
  }

  /**
   * Handles motion events, detects flicks above the threshold,
   * and sends a quick simulated button press when a flick occurs.
   *
   * @param {DeviceMotionEvent} e - The motion event.
   */
  handleMotionEvent(e) {
    const { acceleration } = e;
    if (
      acceleration?.x > this.motionThreshold ||
      acceleration?.y > this.motionThreshold ||
      acceleration?.z > this.motionThreshold
    ) {
      if (!this.isFlickDetected) {
        this.isFlickDetected = true;
        socketManager.updateButton("A", true);
        socketManager.updateButton("A", false);
      }
    } else {
      this.isFlickDetected = false;
    }
  }
}