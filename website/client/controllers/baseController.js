import { Controller } from "./controller.js";

export class BaseController extends Controller {
    constructor(htmlFile, container, socket) {
        super(htmlFile, container);

        this.socket = socket;
    }

    // send input to the unity program
    sendInput(input) {
        console.log(input);
    }
}