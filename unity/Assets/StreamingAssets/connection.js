import { HomeController } from "./controllers/homeController.js";
import { NavController } from "./controllers/navController.js";
import { PlaceHolderController } from "./controllers/placeHolderController.js";

let root = document.querySelector(".view-container");
let navContainer = document.querySelector(".nav-container");
let controllers = [];

let nav = new NavController(navContainer);
controllers.push(nav)
// load home controller by default
// let controller = new HomeController(root);

// if user tries to connect
// Initialize socket
let socket = null;

// listen on socket and load controller based on minigame
let placeh = new PlaceHolderController(root, socket);