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

/**
 * BackgroundRenderer
 *
 * Sets up a 3D scene with Three.js, including camera, lighting, controls,
 * and a live video-textured 3D model, displayed as a background in a container.
 */
export class BackgroundRenderer {
    /**
     * @param {HTMLElement} containerEl - The DOM element to host the Three.js canvas and video feed.
     */
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

    /**
     * Initializes the scene by setting up renderer, scene, camera, controls, lights,
     * the webcam video feed, loading the 3D model and starting the animation loop.
     */
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

    /**
     * Creates a Three.js Scene and sets its background color.
     */
    setupScene() {
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(BG_COLOR_DEFAULT);
    }

    /**
     * Configures a PerspectiveCamera and positions it to view the scene.
     */
    setupCamera() {
        this.camera = new THREE.PerspectiveCamera(
            75,
            window.innerWidth / window.innerHeight,
            0.1,
            1000
        );
        this.camera.position.set(0, 2, 5);
    }

    /**
     * Creates the WebGLRenderer, sizes it to the window, attaches its canvas,
     * and adds a CSS class for styling.
     */
    setupRenderer() {
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.domElement.classList.add('three-background');
        this.container.prepend(this.renderer.domElement);
    }

    /**
     * Adds OrbitControls to allow user interaction with the camera.
     */
    setupControls() {
        const controls = new OrbitControls(this.camera, this.renderer.domElement);
        controls.target.set(0, 1, 0);
        controls.update();
    }

    /**
     * Adds hemisphere and directional lights to the scene for realistic lighting.
     */
    setupLights() {
        const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 1);
        hemi.position.set(0, 20, 0);
        this.scene.add(hemi);

        const dir = new THREE.DirectionalLight(0xffffff, 0.8);
        dir.position.set(3, 10, 10);
        this.scene.add(dir);
    }

    /**
     * Sets up a hidden video element to capture webcam feed,
     * creates a VideoTexture, and resolves when ready or on error.
     *
     * @returns {Promise<void>}
     */
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

    /**
     * Loads an FBX model and applies the video texture to its body,
     * sets up animations, and adds it to the scene.
     *
     * @returns {Promise<void>}
     */
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

    /**
     * Sets the model's material color from predefined options.
     *
     * @param {string} key - Color name key (e.g., 'Blue', 'Red').
     */
    applyColor(key) {
        const hex = MODEL_COLOR_MAP[key] || MODEL_COLOR_MAP.Blue;
        this.modelMaterial.color.set(hex);
    }

    /**
     * Runs the animation loop: updates mixer and renders the scene each frame.
     */
    animate() {
        requestAnimationFrame(() => this.animate());
        const delta = this.clock.getDelta();
        if (this.mixer) this.mixer.update(delta);
        this.renderer.render(this.scene, this.camera);
    }

    /**
     * Updates camera and renderer size on window resize.
     */
    onResize() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }
}