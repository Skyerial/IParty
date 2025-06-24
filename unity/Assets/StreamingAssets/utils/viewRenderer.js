export class ViewRenderer {
    constructor(htmlFile, container) {
        this.htmlFile = htmlFile;
        this.container = container;
    }

    async init() {
        const html = await this.loadHTML(this.htmlFile);
        this.container.innerHTML = html;
        this.bindEvents();
    }

    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.error(`Failed to load HTML file: ${file}`);
        }
        return await response.text();
    }

    bindEvents() {
        // To be overridden in subclass
    }

    getContainer() {
        return this.container;
    }
}
