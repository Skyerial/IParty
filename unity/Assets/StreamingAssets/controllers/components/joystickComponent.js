// JoystickComponent.js
import { ViewRenderer } from '../../utils/viewRenderer.js';
import { socketManager } from '../../main.js';

export class JoystickComponent extends ViewRenderer {
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

  bindEvents() {
    const joystick = this.container.querySelector('#joystick');
    const stick    = this.container.querySelector('.stick');

    joystick.addEventListener('pointerdown', e => {
      if (e.pointerType !== 'touch') return;
      this._pointerId = e.pointerId;
      this.origin.x = e.clientX;
      this.origin.y = e.clientY;
      joystick.setPointerCapture(this._pointerId);
    });

    joystick.addEventListener('pointermove', e => {
      if (e.pointerId !== this._pointerId) return;

      const dx = e.clientX - this.origin.x;
      const dy = e.clientY - this.origin.y;
      const cx = Math.max(-75, Math.min(75, dx));
      const cy = Math.max(-75, Math.min(75, dy));

      stick.style.left = `${75 + cx}px`;
      stick.style.top  = `${75 + cy}px`;

      let x = Math.max(-1, Math.min(1, cx / 75));
      let y = Math.max(-1, Math.min(1, -cy / 75));
      if (!this.vertical) [x, y] = [-y, x];

      socketManager.updateAnalog(x, y);
    });

    joystick.addEventListener('pointerup',     e => this.reset(e));
    joystick.addEventListener('pointercancel', e => this.reset(e));
  }

  reset(e) {
    if (e.pointerId !== this._pointerId) return;

    const joystick = this.container.querySelector('#joystick');
    const stick    = this.container.querySelector('.stick');

    joystick.releasePointerCapture(this._pointerId);
    this._pointerId = null;

    stick.style.left = '75px';
    stick.style.top  = '75px';

    socketManager.updateAnalog(0, 0);
  }
}
