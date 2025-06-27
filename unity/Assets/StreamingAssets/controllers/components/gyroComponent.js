import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

/**
 * @brief Manages device motion input by requesting permission, showing prompts,
 *        and detecting flick gestures to send button press events.
 */
export class GyroComponent extends ViewRenderer {
  /**
   * @brief Constructs a GyroComponent instance.
   * @param {HTMLElement} container - Element where the gyro view will be rendered.
   * @param {boolean} vertical - If true, arranges the UI vertically (unused here).
   * @param {string} displayText - Text to display in the play view.
   */
  constructor(container, vertical, displayText) {
    super("./views/components/gyroComponentView.html", container);
    this.vertical = vertical;
    this.displayText = displayText;
  }

  /**
   * @brief Initializes motion threshold, UI element references, and starts permission flow.
   */
  bindEvents() {
    this.motionThreshold = 15;
    this.isFlickDetected = false;

    this.permissionPrompt = this.container.querySelector("#permissionPrompt");
    this.playView = this.container.querySelector("#playView");
    this.enableButton = this.container.querySelector("#enableButton");

    const text = this.container.querySelector("#displayText");
    text.innerHTML = this.displayText;

    this.requestMotionPermission();
  }

  /**
   * @brief Requests permission for device motion events if needed, then shows the play view
   *        and starts tracking, or shows the permission prompt on denial.
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
   * @brief Displays the main play UI and hides the permission prompt.
   */
  showPlayView() {
    this.permissionPrompt.style.display = "none";
    this.playView.style.display = "block";
  }

  /**
   * @brief Displays the permission prompt and sets up the enable button listener.
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
   * @brief Starts listening to device motion events for flick detection.
   */
  startMotionTracking() {
    window.addEventListener(
      "devicemotion",
      this.handleMotionEvent.bind(this),
      true
    );
  }

  /**
   * @brief Handles motion events, detects flicks above the threshold,
   *        and sends a simulated button press ('A') when a flick occurs.
   * @param {DeviceMotionEvent} e - The motion event payload.
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
