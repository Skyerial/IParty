# iParty

## Overview
iParty is a Unity-based digital party game where you use your mobile phone as your controller! Player compete in minigames to gain an advantage in moving higher in the world and to win it all. This project was developed for the Project Software Engineering course at the University of Amsterdam.

[Check out our website!](http://83.96.253.147/)

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
doxygen Doxyfile
```

### Known bugs

- Ranking in the Tank minigame is reversed
- Upon joining without enabling Gyro, some players might not spawn in the gameboard.
- Skyglutes occasionally stops some players from spawning, skipping their turn.
