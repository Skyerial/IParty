/**
 * ViewRenderer
 *
 * A simple class that loads an HTML file and displays it inside a given container element.
 * Child classes can customize how events are set up after the HTML is loaded.
 */
export class ViewRenderer {
    /**
     * Sets up a new ViewRenderer instance.
     *
     * @param {string} htmlFile - The path or URL of the HTML file to load.
     * @param {HTMLElement} container - The element where the HTML content will appear.
     */
    constructor(htmlFile, container) {
        this.htmlFile = htmlFile;
        this.container = container;
    }

    /**
     * Loads the HTML content and displays it, then calls the method to set up events.
     *
     * @async
     * @returns {Promise<void>} Completes when content is shown and events are set up.
     */
    async init() {
        const html = await this.loadHTML(this.htmlFile);
        this.container.innerHTML = html;
        this.bindEvents();
        this.reconnectEvent();
    }

    /**
     * Fetches the HTML from the specified location.
     * Logs an error if the fetch fails, but still returns whatever text is received.
     *
     * @async
     * @param {string} file - The HTML file path or URL.
     * @returns {Promise<string>} The raw HTML as a string.
     */
    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.error(`Failed to load HTML file: ${file}`);
        }
        return await response.text();
    }

    /**
     * Sets up event listeners on the rendered content.
     * Child classes should override this method to add their own event handling.
     */
    bindEvents() {
        // Override in child classes to attach event handlers
    }

    /**
     * Returns the container element where the HTML is rendered.
     *
     * @returns {HTMLElement} The container element.
     */
    
    reconnectEvent() {
        // to be overridden in subclass
    }

    getContainer() {
        return this.container;
    }
}
