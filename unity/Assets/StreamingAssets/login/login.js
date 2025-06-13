export class Login {
    constructor(container) {
        this.container = container
    }

    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.error(`Failed to load ${file}`);
        }
        return await response.text();
    }

    async init() {
        const html = await this.loadHTML("/login/index.html");
        this.container.innerHTML = html;
        
        // Three
        var three = document.createElement("script")
        three.src = "https://unpkg.com/three@0.177.0/build/three.min.js"

        // Importmap
        var map = document.createElement("script")
        map.type = "importmap"
        map.textContent =`{
            "imports": {
                "three": "https://threejs.org/build/three.module.js",
                "three/addons/": "https://threejs.org/examples/jsm/"
            }}`

        // forum.js
        var forum = document.createElement("script")
        forum.type = "module"
        forum.src = "/login/forum.js"

        // background_renderer.js
        var back = document.createElement("script")
        back.type = "module"
        back.src = "/login/background_render.js"


        this.container.append(three)
        this.container.append(map)
        this.container.append(forum)
        this.container.append(back)
    }
}