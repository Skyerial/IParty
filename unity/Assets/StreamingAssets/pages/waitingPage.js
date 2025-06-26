import { ViewRenderer } from "../utils/viewRenderer.js";

export class WaitingPage extends ViewRenderer {

  constructor(container) {
    super("./views/waitingView.html", container);
  }
}