import { ViewRenderer } from '../utils/viewRenderer.js';
import { BackgroundRenderer } from '../utils/backgroundRenderer.js';
import { socketManager } from '../main.js';

const MAX_NAME_LENGTH = 12;

/**
 * @brief Renders a login interface with live video background.
 * @details Allows the user to take or retake a snapshot, select a color, and submit their details.
 * @extends ViewRenderer
 */
export class LoginPage extends ViewRenderer {
  /**
   * @brief Constructs a LoginPage.
   * @param {HTMLElement} container - Element where the login view will be displayed.
   */
  constructor(container) {
    super('views/loginView.html', container);
  }

  /**
   * @brief Called after the view loads; initializes the background video renderer,
   *        finds input elements, and sets up event listeners for snapshot,
   *        color selection, and form submission.
   */
  bindEvents() {
    const loginContainer = this.container.querySelector('.login-container');
    if (!loginContainer) throw new Error('Login container not found');

    // Initialize live video background
    this.bgRenderer = new BackgroundRenderer(loginContainer);
    this.bgRenderer.init();
    this.video = this.bgRenderer.video;

    // Cache UI elements
    this.nameInput     = loginContainer.querySelector('#user');
    this.colorButtons  = Array.from(loginContainer.querySelectorAll('.color-box'));
    this.snapButton    = loginContainer.querySelector('#picture-snap');
    this.playButton    = loginContainer.querySelector('#play-button');

    // Bind snapshot toggle
    this.snapButton.addEventListener('click', () =>
      this.toggleSnapshot(loginContainer)
    );

    // Bind color selection
    this.colorButtons.forEach(btn =>
      btn.addEventListener('click', () => this.onColorSelect(btn.id))
    );

    // Bind form submission
    this.playButton.addEventListener('click', () => this.submitPlayer());
  }

  /**
   * @brief Toggles between live video and snapshot mode.
   * @param {HTMLElement} loginContainer - The container for the login UI.
   * @details
   * - In live mode: shows "Take Picture" and removes any existing canvas.
   * - In snapshot mode: pauses video, captures current frame to canvas,
   *   displays "Retake Picture" button, and stores the canvas element.
   */
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
      canvas.width  = this.video.videoWidth;
      canvas.height = this.video.videoHeight;
      canvas.getContext('2d').drawImage(this.video, 0, 0);
      loginContainer.appendChild(canvas);
      this.snapshotCanvas = canvas;
    }
  }

  /**
   * @brief Handles color button clicks: highlights the selected color,
   *        stores the selection, and applies it to the background renderer.
   * @param {string} colorKey - The key identifying the chosen color.
   */
  onColorSelect(colorKey) {
    this.colorButtons.forEach(b =>
      b.classList.toggle('selected', b.id === colorKey)
    );
    this.selectedColor = colorKey;
    this.bgRenderer.applyColor(colorKey);
  }

  /**
   * @brief Validates input, extracts snapshot data if available,
   *        and sends the player’s name, selected color, and image data via socket.
   */
  submitPlayer() {
    const name = this.nameInput.value.trim();
    if (!name) return alert('Please enter a player name.');
    if (name.length > MAX_NAME_LENGTH)
      return alert('Name can’t be longer than 12 characters');
    if (!this.selectedColor) 
      return alert('Please select a color.');

    const data = this.snapshotCanvas
      ? this.snapshotCanvas
          .toDataURL('image/png')
          .replace(/^data:image.+;base64,/, '')
      : '';

    socketManager.send({ name, color: this.selectedColor, data });
    this.bgRenderer.stop();
  }
}
