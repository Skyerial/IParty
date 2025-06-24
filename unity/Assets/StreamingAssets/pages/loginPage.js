import { ViewRenderer } from '../utils/viewRenderer.js';
import { BackgroundRenderer } from '../utils/backgroundRenderer.js';
import { socketManager } from '../main.js';

export class LoginPage extends ViewRenderer {
  constructor(container) {
    super('views/loginView.html', container);
  }

  bindEvents() {
    const loginContainer = this.container.querySelector('.login-container');
    if (!loginContainer) throw new Error('Login container not found');
    this.bgRenderer = new BackgroundRenderer(loginContainer);
    this.bgRenderer.init();
    this.video = this.bgRenderer.video;
    this.nameInput = loginContainer.querySelector('#user');
    this.colorButtons = Array.from(loginContainer.querySelectorAll('.color-box'));
    this.snapButton = loginContainer.querySelector('#picture-snap');
    this.playButton = loginContainer.querySelector('#play-button');

    this.snapButton.addEventListener('click', () => this.toggleSnapshot(loginContainer));
    this.colorButtons.forEach(btn =>
      btn.addEventListener('click', () => this.onColorSelect(btn.id))
    );
    this.playButton.addEventListener('click', () => this.submitPlayer());
  }

  toggleSnapshot(loginContainer) {
    if (this.video.paused) {
      this.video.play();
      this.snapButton.textContent = 'Take Picture';
      this.snapshotCanvas?.remove();
    } else {
      this.video.pause();
      this.snapButton.textContent = 'Retake Picture';
      const canvas = document.createElement('canvas');
      canvas.id = 'snapshot';
      canvas.width = this.video.videoWidth;
      canvas.height = this.video.videoHeight;
      canvas.getContext('2d').drawImage(this.video, 0, 0);
      loginContainer.appendChild(canvas);
      this.snapshotCanvas = canvas;
    }
  }

  onColorSelect(colorKey) {
    this.colorButtons.forEach(b =>
      b.classList.toggle('selected', b.id === colorKey)
    );
    this.selectedColor = colorKey;
    this.bgRenderer.applyColor(colorKey);
  }

  submitPlayer() {
    const name = this.nameInput.value.trim();
    if (!name) return alert('Please enter a player name.');
    if (!this.selectedColor) return alert('Please select a color.');

    const data = this.snapshotCanvas
      ? this.snapshotCanvas.toDataURL('image/png').replace(/^data:image.+;base64,/, '')
      : '';

    socketManager.send({ name, color: this.selectedColor, data });
  }
}