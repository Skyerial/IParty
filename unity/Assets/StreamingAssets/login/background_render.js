import * as THREE from 'three';
import { OrbitControls } from 'https://unpkg.com/three@0.177.0/examples/jsm/controls/OrbitControls.js';
import { FBXLoader } from 'https://unpkg.com/three@0.177.0/examples/jsm/loaders/FBXLoader.js';

// Make a material
let modelMaterial = new THREE.MeshStandardMaterial({
    color: 0x00CCF0, // RED (you can use hex, CSS-style strings, or THREE.Color)
    metalness: 0.1,
    roughness: 0.5
  });

// Activate change of color
export function color_change(selectedColor) {
    if (selectedColor == "Blue") {
        modelMaterial.color.set(0x00CCF0)
    } else if (selectedColor == "Yellow") {
        modelMaterial.color.set(0xC7A900)
    } else if (selectedColor == "Green") {
        modelMaterial.color.set(0x65DB05)
    } else if (selectedColor == "Red") {
        modelMaterial.color.set(0xB30505)
    }
}

function main() {
    // Scene
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x687cff);

    // Camera
    const camera = new THREE.PerspectiveCamera(
        75, window.innerWidth / window.innerHeight, 0.1, 1000
    );
    camera.position.set(0, 2, 5);

    // Renderer
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(window.innerWidth, window.innerHeight);
    const container = document.getElementById("model-background")
    container.appendChild(renderer.domElement);

    // Controls
    const controls = new OrbitControls(camera, renderer.domElement);
    controls.target.set(0, 1, 0);
    controls.update();

    // Lighting
    const hemiLight = new THREE.HemisphereLight(0xffffff, 0x444444, 1);
    hemiLight.position.set(0, 20, 0);
    scene.add(hemiLight);

    const dirLight = new THREE.DirectionalLight(0xffffff, 0.8);
    dirLight.position.set(3, 10, 10);
    scene.add(dirLight);

    // Loading live video feed
    const video = document.getElementById('webcam');
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
      navigator.mediaDevices.getUserMedia({ 
        video: {
          width: {ideal: 480},
          height: {ideal: 640},
        }
      })
        .then((stream) => {
          const video = document.getElementById('webcam');
          video.srcObject = stream;
          video.play();
        })
        .catch((err) => {
          console.error('Webcam access denied or failed:', err);
        });
    } else {
      console.error('getUserMedia is not supported in this browser.');
    }

    const snapButton = document.getElementById('picture-snap');
    // Checking if snapshot taken
    snapButton.addEventListener('click', () => {
      if (video.paused) {
        video.play();
        snapButton.innerHTML = "Take Picture";
      } else {
        video.pause();
        snapButton.innerHTML = "Retake Picture";
      }
    });

    const saveButton = document.getElementById('picture-save');
    saveButton.addEventListener('click', () => {
      const canvas = document.createElement('canvas');
      canvas.height = video.videoHeight;
      canvas.width = video.videoWidth;
      canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);

      const img = canvas.toDataURL('image/png');
      
      const link = document.createElement('a');
      link.href = img;
      link.download = 'captured-image.png'; // File name
      link.click(); // Trigger the download
    })

    // Making a texture out of it
    const videoTexture = new THREE.VideoTexture(video);
    videoTexture.minFilter = THREE.LinearFilter;
    videoTexture.magFilter = THREE.LinearFilter;
    videoTexture.format = THREE.RGBFormat;

    // Load FBX model
    let mixer;
    const loader = new FBXLoader();
    loader.load(
      './login/player.fbx', // Your FBX file
      (object) => {
        object.scale.set(0.01, 0.01, 0.01);
        object.traverse((child) => {
          if (child.isMesh) {
            // Ensure material is a standard material (or convert it)
            child.material = modelMaterial;
          }
          const part = object.getObjectByName('Body001');
          part.material = new THREE.MeshStandardMaterial({
            map: videoTexture,
          });
        });

        const anim = new FBXLoader();
        anim.load('./login/IdleW.fbx', (anim) => {
          mixer = new THREE.AnimationMixer(object);
          const action = mixer.clipAction(anim.animations[0]);
          action.play();
        });
        scene.add(object);
      },
      (xhr) => {
        console.log((xhr.loaded / xhr.total) * 100 + '% loaded');
      },
      (error) => {
        console.error('Error loading FBX:', error);
      }
    );

    const clock = new THREE.Clock();

    // Render loop
    function animate() {
      requestAnimationFrame(animate);

      const delta = clock.getDelta();
      if (mixer) mixer.update(delta);

      renderer.render(scene, camera);
    }

    animate();

    // Resize handling
    window.addEventListener('resize', () => {
        camera.aspect = window.innerWidth / window.innerHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(window.innerWidth, window.innerHeight);
    });
}

main();
