import { LayoutManager } from "../utils/layoutManager.js";

export class ReadyController {
    constructor(container) {
        this.container = container
    }

    async init() {
        const layout = new LayoutManager(this.container, false);
        await layout.init();

        await layout.addButton({
            text:       'READY',
            className: 'green-ready'
        });
    }
}