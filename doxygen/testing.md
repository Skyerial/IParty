# Testing

Testing minigames and the game logic can be tough without access to multiple devices to supply inputs. To aid with this we developed a script that registers up to 4 players and sends random inputs for each on a loop. Using this tool one tester may find problems in the game loop by themselves while avoiding any connection bugs. The testing script does not pass certifications from later security checks for wss. To use it, change wss to ws in ServerManager.cs and socketManager.js or switch to a dedicated branch.

Aside from that, we ran in person tests throughout the final week. Test reports can be found in our [google drive](./files.md) map under Tests.

---

<small>
Developed by Group E for the University of Amsterdam.

Owen Duddles, Liam Gatersleben, Tom Groot, Willem Haasdijk, Akbar Ismatullayev, Saleeman Mahamud, Ryoma Nonaka, Daniel Oppenhuizen, Scott Scherpenzeel, Jonas Skolnik, Boris Vukajlovic, Narek Wartanian, Jasper Wormsbecher
</small>
