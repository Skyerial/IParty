import * as THREE from 'https://unpkg.com/three@0.177.0/build/three.module.js';
import { OrbitControls } from 'https://unpkg.com/three@0.177.0/examples/jsm/controls/OrbitControls.js';
import { FBXLoader } from 'https://unpkg.com/three@0.177.0/examples/jsm/loaders/FBXLoader.js';

const MODEL_COLOR_MAP = {
  Blue:   0x00ccf0,
  Yellow: 0xc7a900,
  Green:  0x65db05,
  Red:    0xb30505
};

const BG_COLOR_DEFAULT = 0x687cff;

export class BackgroundRenderer {
  constructor(containerEl) {
    this.container = containerEl;
    this.clock = new THREE.Clock();
    this.mixer = null;
    this.modelMaterial = new THREE.MeshStandardMaterial({
      color: MODEL_COLOR_MAP.Blue,
      metalness: 0.1,
      roughness: 0.5
    });
  }

  async init() {
    this.setupScene();
    this.setupCamera();
    this.setupRenderer();
    this.setupControls();
    this.setupLights();
    await this.setupVideoFeed();
    await this.loadModel();
    window.addEventListener('resize', () => this.onResize());
    this.animate();
  }

  setupScene() {
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(BG_COLOR_DEFAULT);
  }

  setupCamera() {
    this.camera = new THREE.PerspectiveCamera(
      75,
      window.innerWidth / window.innerHeight,
      0.1,
      1000
    );
    this.camera.position.set(0, 2, 5);
  }

  setupRenderer() {
    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    // give the canvas a class so it doesn't style snapshot canvas
    this.renderer.domElement.classList.add('three-background');
    this.container.prepend(this.renderer.domElement);
  }

  setupControls() {
    const controls = new OrbitControls(this.camera, this.renderer.domElement);
    controls.target.set(0, 1, 0);
    controls.update();
  }

  setupLights() {
    const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 1);
    hemi.position.set(0, 20, 0);
    this.scene.add(hemi);

    const dir = new THREE.DirectionalLight(0xffffff, 0.8);
    dir.position.set(3, 10, 10);
    this.scene.add(dir);
  }

  setupVideoFeed() {
    return new Promise((resolve) => {
      this.video = document.createElement('video');
      this.video.id = 'webcam';
      this.video.autoplay = true;
      this.video.playsInline = true;
      this.video.style.display = 'none';
      this.container.prepend(this.video);

      if (navigator.mediaDevices?.getUserMedia) {
        navigator.mediaDevices
          .getUserMedia({ video: { width: { ideal: 480 }, height: { ideal: 640 } } })
          .then(stream => {
            this.video.srcObject = stream;
            this.video.play();
            this.videoTexture = new THREE.VideoTexture(this.video);
            this.videoTexture.minFilter = THREE.LinearFilter;
            this.videoTexture.magFilter = THREE.LinearFilter;
            this.videoTexture.format = THREE.RGBFormat;
            resolve();
          })
          .catch(err => {
            console.error('Webcam access failed:', err);
            resolve();
          });
      } else {
        console.error('getUserMedia not supported');
        resolve();
      }
    });
  }

  loadModel() {
    return new Promise((resolve, reject) => {
      const loader = new FBXLoader();
      loader.load(
        './utils/player.fbx',
        object => {
          object.scale.set(0.01, 0.01, 0.01);
          object.position.y = 1.5;

          object.traverse(child => {
            if (child.isMesh) child.material = this.modelMaterial;
            const part = object.getObjectByName('Body001');
            if (part) {
              part.material = new THREE.MeshStandardMaterial({ map: this.videoTexture });
            }
          });

          const animLoader = new FBXLoader();
          animLoader.load('./utils/IdleW.fbx', anim => {
            this.mixer = new THREE.AnimationMixer(object);
            this.mixer.clipAction(anim.animations[0]).play();
          });

          this.scene.add(object);
          resolve();
        },
        undefined,
        err => reject(err)
      );
    });
  }

  applyColor(key) {
    const hex = MODEL_COLOR_MAP[key] || MODEL_COLOR_MAP.Blue;
    this.modelMaterial.color.set(hex);
  }

  animate() {
    requestAnimationFrame(() => this.animate());
    const delta = this.clock.getDelta();
    if (this.mixer) this.mixer.update(delta);
    this.renderer.render(this.scene, this.camera);
  }

  onResize() {
    this.camera.aspect = window.innerWidth / window.innerHeight;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(window.innerWidth, window.innerHeight);
  }
}
