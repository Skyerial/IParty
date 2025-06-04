export class Controller {

    constructor(htmlFile, container) {
        this.htmlFile = htmlFile;
        this.container = container;
        this.init();
    }

    // Initialize and render view
    async init() {
        const html = await this.loadHTML(this.htmlFile);
        this.container.innerHTML = html;
        this.bindEvents();
    }

    // fetch and read html file
    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.log(`Failed to load ${file}`);
        }

        return await response.text();
    }

    // overide in subclasses
    bindEvents() {}
}