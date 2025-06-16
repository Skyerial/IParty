import { socketManager } from "../../main.js";
import { ViewRenderer } from "../../utils/viewRenderer.js";

export class JoystickComponent extends ViewRenderer{
  constructor(container, vertical = false) {
    super("./views/components/joystickComponentView.html", container, "analogInput");
    this.vertical  = vertical;
    this.origin = { x: 0, y: 0 };
  }

  bindEvents() {
    const joystick = this.container.querySelector('#joystick');
    const stick    = this.container.querySelector('.stick');

    joystick.addEventListener('touchstart', e => {
      const t = e.touches[0];
      this.origin.x = t.clientX;
      this.origin.y = t.clientY;
    });

    joystick.addEventListener('touchmove', e => {
      e.preventDefault();
      const t  = e.touches[0];
      const dx = t.clientX - this.origin.x;
      const dy = t.clientY - this.origin.y;
      const cx = Math.max(-75, Math.min(75, dx));
      const cy = Math.max(-75, Math.min(75, dy));

      stick.style.left = `${75 + cx}px`;
      stick.style.top  = `${75 + cy}px`;

      let x = Math.max(-1, Math.min(1, cx / 75));
      let y = Math.max(-1, Math.min(1, -cy / 75));

      // rotate axes for horizontal dpad if needed
      if (!this.vertical) [x, y] = [-y, x];

      socketManager.updateAnalog(x, y);
    }, { passive: false });

    joystick.addEventListener('touchend', () => {
      stick.style.left = '75px';
      stick.style.top  = '75px';
      socketManager.updateAnalog(0, 0);
    });
  }
}
