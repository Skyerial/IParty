import { SocketManager } from "./utils/socketManager.js";
import { ConnectPage } from "./pages/connectPage.js";
import { JoystickController } from "./pages/joystickController.js";
import { NavBar } from "./pages/navBar.js";

// Create the shared socket manager instance
export const socketManager = new SocketManager();

window.addEventListener("DOMContentLoaded", async () => {
  const root = document.querySelector(".view-container");
  const navContainer = document.querySelector(".nav-container");

  const nav = new NavBar(navContainer);
  await nav.init();

  const params = new URLSearchParams(window.location.search);
  const code = params.get("hostId");

  if (code && !socketManager.isConnected()) {
    socketManager.connect(code);

    socketManager.setHandlers({
      onMessage: (data) => {
        console.log("📩 Received:", data);
        // Optionally route messages to a controller
      },
      onOpen: () => {
        console.log("🟢 Socket connected");
      },
      onClose: () => {
        console.log("🔴 Socket disconnected");
      }
    });

    const controller = new JoystickController(root, socketManager);
    await controller.init();
  } else {
    const connectPage = new ConnectPage(root);
    await connectPage.init();
  }
});
