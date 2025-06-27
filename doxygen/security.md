# Security
Even for a game, security is still has quite some significant importance. Cybersecurity issues in the gaming industry has been growing every year with ddos attacks and botting still being the worst. As a group we considered implementation methods that allowed for seamless connection with the client whilst maintaining a secure platform. We decided that a safe implementation between client and host connections was best done using a secure websocket tunnel and a https listener. This allowed us to control the flow of incoming connections in a safe and secure manner using tls for https using x509 to sign our websocket connections. The relay server also implemented a safe passthrough of the data both to the client and the host, only allowing messages to be sent to and from hosts and clients which established a proper connection. It also has some rate limiting implemented in order to ensure no one can flood/overload the server using false connections. On top of that the host filters incoming JSONs to make sure noone has tampered with the input from controllers. There were some security features to be implemented, like tokenizing the client connections to make sure noone could spoof a connection but sadly became out of scope during the course. We made sure our source code was not public during development as this can lead to source code disclosure and could me misused by a malicious actor by i.e. finding dev keys accidentally left in during production or fuzzing our software. Our server only allows ufw connections on the game port 5000, 5001 and 5002.

---

<small>
Developed by Group E for the University of Amsterdam.

Owen Duddles, Liam Gatersleben, Tom Groot, Willem Haasdijk, Akbar Ismatullayev, Saleeman Mahamud, Ryoma Nonaka, Daniel Oppenhuizen, Scott Scherpenzeel, Jonas Skolnik, Boris Vukajlovic, Narek Wartanian, Jasper Wormsbecher
</small>
