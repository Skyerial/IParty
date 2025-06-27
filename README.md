# iParty

## Overview
iParty is a Unity-based digital party game where you use your mobile phone as your controller! Player compete in minigames to gain an advantage in moving higher in the world and to win it all. This project was developed for the Project Software Engineering course at the University of Amsterdam.

Check out [our website](http://83.96.253.147/)!<br>
[Links](http://83.96.253.147/d2/d15/md_files.html)<br>
[Testing](http://83.96.253.147/de/d6a/md_testing.html)<br>
[Green Thinking](http://83.96.253.147/de/dc7/md_greenthinking.html)<br>
[Security](http://83.96.253.147/d3/d79/md_security.html)<br>
[Adding your own game](http://83.96.253.147/db/d3d/md_buildinggame.html)<br>


## Project Structure
- `unity/` - Contains the Unity project, including all game assets, scripts, and scenes.
- `testing/` - Contains scripts and tools for automated and manual testing, including `fourplayertest.py` and setup files.
- `doxygen/` - Contains Doxygen configuration (`Doxyfile`) and generated documentation output.

## Unity
The game is implemented in Unity (see [`unity/`](unity/)).
Players move around a virtual board, triggering events and playing minigames. The codebase is organized into scripts for board logic, player control, minigames, and UI.

## Tester
The [`testing/`](testing/) folder contains tools for automated testing. See the header in fourplayertest.py for details. Functionality is deprecated since switching to wss but works in the testconnections branch.

## Doxygen
Contains Doxyfile that automatically generates documentation. Result can be found at http://83.96.253.147/ or may be generated using Doxyfile.

Requirements:
```bash
sudo apt install doxygen
sudo apt install graphviz
```

Create documentation:
```bash
doxygen doxygen/Doxyfile
```

### Known bugs
Most bugs can be stopped by preventing reconnection is needed. Reconnection works on local most of the time but can cause bugs to happen and does not work when playing remotely. Other bugs are:

- Sometimes an issue arrises causing the players not to not load in the gameboard at the start. A simple restart fixes this, if not, run the remote server to play.
- The remote server does not allow reconnections; leaving the browser causes the player unable to play.
- Skyglutes occasionally stops some players from spawning, skipping their turn or even fataly stopping the game.
- Random reconnection prompt (rarely happens)
- Moving back and forth from Lobby to Main Menu stops QR-code generation
- Starting the game too quickly after connecting a player stops them from loading in as gyro is certified.
