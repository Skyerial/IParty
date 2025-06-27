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
 * @brief Sets up a Three.js scene with lighting, controls, webcam-textured model, and handles rendering loop.
 */
export class BackgroundRenderer {
    /**
     * @brief Constructs a BackgroundRenderer for the given container element.
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

        this.stream = null;
    }

    /**
     * @brief Initializes the renderer: scene, camera, lights, video feed, model loading, and starts animation.
     * @returns {Promise<void>}
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
     * @brief Creates a Three.js Scene and sets its background color.
     */
    setupScene() {
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(BG_COLOR_DEFAULT);
    }

    /**
     * @brief Configures a PerspectiveCamera and positions it to view the scene.
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
     * @brief Creates the WebGLRenderer, sizes it to the window, and attaches its canvas to the container.
     */
    setupRenderer() {
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.domElement.classList.add('three-background');
        this.container.prepend(this.renderer.domElement);
    }

    /**
     * @brief Adds OrbitControls to allow user interaction with the camera.
     */
    setupControls() {
        const controls = new OrbitControls(this.camera, this.renderer.domElement);
        controls.target.set(0, 1, 0);
        controls.update();
    }

    /**
     * @brief Adds hemisphere and directional lights for realistic illumination.
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
     * @brief Captures webcam feed into a hidden video element and creates a VideoTexture.
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
                        this.stream = stream;
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
     * @brief Loads an FBX model, applies video texture and animations, then adds it to the scene.
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
     * @brief Sets the model's material color from predefined MODEL_COLOR_MAP.
     * @param {string} key - Color name key (e.g., 'Blue', 'Red').
     */
    applyColor(key) {
        const hex = MODEL_COLOR_MAP[key] || MODEL_COLOR_MAP.Blue;
        this.modelMaterial.color.set(hex);
    }

    /**
     * @brief Runs the animation loop: updates mixer and renders the scene each frame.
     */
    animate() {
        requestAnimationFrame(() => this.animate());
        const delta = this.clock.getDelta();
        if (this.mixer) this.mixer.update(delta);
        this.renderer.render(this.scene, this.camera);
    }

    /**
     * @brief Adjusts camera aspect and renderer size on window resize.
     */
    onResize() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }

    /**
     * @brief Stops the webcam stream, removes video element, and disposes of the VideoTexture.
     */
    stop() {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        if (this.video) {
            this.video.srcObject = null;
            this.video.remove();
            this.video = null;
        }

        if (this.videoTexture) {
            this.videoTexture.dispose();
            this.videoTexture = null;
        }
    }
}
