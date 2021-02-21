# Distributed Password Cracker

A distributed password cracker made as part of the IT security course at Erhvervsakademi Sjælland.

It consists of a server program that is the central component of the system and a client that connects to it. It is written in C# and .NET Core.

The server opens a dictonary file and splits it into chunks based on the number of clients specified. Then, for every client, it runs a new thread and listens for a TCP connection. When a client connects, the server sends to it one chunk of the dictionary and the list of passwords to be cracked. Then it waits for the client's response.

Every client tries to crack the passwords by hashing the words in the chunk of the dictionary that was sent to it, and then comparing the hash with the password. If they are the same, that means that word is the password. The client runs multiple tasks to speed up the process. Finally, the results are sent to the server.
