// JoystickComponent.js
import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

/**
 * JoystickComponent renders a virtual joystick UI that captures touch input
 * and emits normalized analog values through the socketManager.
 */
export class JoystickComponent extends ViewRenderer {
  /**
   * Create a JoystickComponent.
   *
   * @param {HTMLElement} container - The DOM element to render the joystick into.
   * @param {boolean} [vertical=false] - If true, invert axes for a vertical layout.
   */
  constructor(container, vertical = false) {
    super(
      "./views/components/joystickComponentView.html",
      container,
      "analogInput"
    );
    this.vertical = vertical;
    this._pointerId = null;
    this.origin = { x: 0, y: 0 };
  }

  /**
   * Bind pointer event listeners to the joystick elements.
   * Handles pointerdown, pointermove, pointerup, and pointercancel.
   */
  bindEvents() {
    const joystick = this.container.querySelector('#joystick');
    const stick    = this.container.querySelector('.stick');

    // On touch start: capture pointer and record origin
    joystick.addEventListener('pointerdown', e => {
      if (e.pointerType !== 'touch') return;
      this._pointerId = e.pointerId;
      this.origin.x = e.clientX;
      this.origin.y = e.clientY;
      joystick.setPointerCapture(this._pointerId);
    });

    // On touch move: calculate displacement, update stick position, and send analog values
    joystick.addEventListener('pointermove', e => {
      if (e.pointerId !== this._pointerId) return;

      const dx = e.clientX - this.origin.x;
      const dy = e.clientY - this.origin.y;
      // Clamp displacement within joystick radius (±75px)
      const cx = Math.max(-75, Math.min(75, dx));
      const cy = Math.max(-75, Math.min(75, dy));

      // Position stick graphic relative to base
      stick.style.left = `${75 + cx}px`;
      stick.style.top  = `${75 + cy}px`;

      // Normalize to [-1, 1]; invert Y-axis for intuitive control
      let x = Math.max(-1, Math.min(1, cx / 75));
      let y = Math.max(-1, Math.min(1, -cy / 75));
      if (!this.vertical) [x, y] = [-y, x];

      // Emit updated analog values
      socketManager.updateAnalog(x, y);
    });

    // On touch end or cancel: reset joystick
    joystick.addEventListener('pointerup',     e => this.reset(e));
    joystick.addEventListener('pointercancel', e => this.reset(e));
  }

  /**
   * Reset the joystick to its neutral position and emit zeroed analog input.
   *
   * @param {PointerEvent} e - The pointer event triggering the reset.
   */
  reset(e) {
    if (e.pointerId !== this._pointerId) return;

    const joystick = this.container.querySelector('#joystick');
    const stick    = this.container.querySelector('.stick');

    // Release pointer capture and clear stored pointer id
    joystick.releasePointerCapture(this._pointerId);
    this._pointerId = null;

    // Center the stick graphic
    stick.style.left = '75px';
    stick.style.top  = '75px';

    // Emit neutral analog values
    socketManager.updateAnalog(0, 0);
  }
}

