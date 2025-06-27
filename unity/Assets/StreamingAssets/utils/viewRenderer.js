/**
 * @brief A simple class that loads an HTML file into a container and allows subclasses to bind events.
 * @details Fetches and injects HTML content, then calls bindEvents and reconnectEvent hooks.
 */
export class ViewRenderer {
    /**
     * @brief Constructs a new ViewRenderer.
     * @param {string} htmlFile - The path or URL of the HTML file to load.
     * @param {HTMLElement} container - The element where the HTML content will be injected.
     */
    constructor(htmlFile, container) {
        this.htmlFile = htmlFile;
        this.container = container;
    }

    /**
     * @brief Initializes the view by loading HTML, injecting it, and binding events.
     * @async
     * @returns {Promise<void>} Resolves when content is loaded and events are set up.
     */
    async init() {
        const html = await this.loadHTML(this.htmlFile);
        this.container.innerHTML = html;
        this.bindEvents();
        this.reconnectEvent();
    }

    /**
     * @brief Fetches HTML content from the specified file or URL.
     * @async
     * @param {string} file - The HTML file path or URL.
     * @returns {Promise<string>} The fetched HTML as a string.
     */
    async loadHTML(file) {
        const response = await fetch(file);
        if (!response.ok) {
            console.error(`Failed to load HTML file: ${file}`);
        }
        return await response.text();
    }

    /**
     * @brief Hook for subclasses to attach event listeners after HTML is injected.
     * @remarks Override in child classes to implement specific event bindings.
     */
    bindEvents() {
        // Override in child classes to attach event handlers
    }

    /**
     * @brief Hook for subclasses to handle reconnection logic after view initialization.
     * @remarks Override in child classes if needed.
     */
    reconnectEvent() {
        // to be overridden in subclass
    }

    /**
     * @brief Retrieves the container element used for rendering.
     * @returns {HTMLElement} The container element.
     */
    getContainer() {
        return this.container;
    }
}
