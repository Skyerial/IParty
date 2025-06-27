# Building your own game

To add a new minigame to the project, follow these steps:

Create your minigame scene.
This is where your gameplay takes place. Each player should interact with the game using a prefab that contains a PlayerInput component.

Don't add a PlayerInputManager.
The server already handles device pairing and player joining. You only need to spawn one prefab per player, so do not add a PlayerInputManager to your scene.

Use a spawner to instantiate players.
Add a spawner object to your scene with a script that:

- Specifies which prefab to use (so where the playerinput is)

- Instantiates one instance per connected player

- Assigns the correct input device

Create a manager for your minigame.
This manager (e.g. GameManagerMyGame) should coordinate gameplay logic and UI, and optionally handle assigning player colors, scores, etc.

Add your game to the random selector.
Register your scene in the minigame selector system so it can be randomly chosen during the main game loop.

Provide a short description.
This links your minigame to the gameboard explanation screen. Since this loads the selector and after the description it loads your minigame.

Create a controller for your minigame:
    On the client side, create a new controller class or check if an existing one meets your requirements. Instantiate a new LayoutManager object and add the components you need. Once the class is ready, you can create an instance of it and initialize it to display the controller. On the backend, make sure to include the controller name in the message sent using ServerManager.SendToAllSockets(). Then, add that controller name to the map in socketManager.js so it can be properly recognized and handled on the client.

---

<small>
Developed by Group E for the University of Amsterdam.

Owen Duddles, Liam Gatersleben, Tom Groot, Willem Haasdijk, Akbar Ismatullayev, Saleeman Mahamud, Ryoma Nonaka, Daniel Oppenhuizen, Scott Scherpenzeel, Jonas Skolnik, Boris Vukajlovic, Narek Wartanian, Jasper Wormsbecher
</small>

