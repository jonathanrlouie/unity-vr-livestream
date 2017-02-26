ServerClient_setup

Version 0.3_(5.4) : Included Command Rpc and SyncVar examples.
        0.2 : Improved Password system, added option to set maximum connections.
        0.1 : Initial release.

This setup will give you a (Dedicated)Server and Client in one Build.
Run one build as a Server on any PC any where and use others as a Client to connect to the server via IP address.

A little setup is required:

1 Add all the scenes to the "Build Settings"".
2 Make sure the "Manager_Network" Gameobject in the "MainMenu" scene has the TAG "NetworkManager".
  (when using Unity 5.3.4 it looked like the tag was added when importing the package but it wasn`t really there,
  So click on the tagglist>addtag and see if its really there, if not create it and add it to the "Manager_Network" Object).
3 In the PlayerSettings turn on "Run In Background".
4 (For people outside your Local network to be able to connect)
  Port forward the port the Server uses in your Router/modem settings to the PC the Server is on. 

That`s it..  you`re good to go.


What is this package about:

It`s basicly a replacement for the standard Unity Networking Manager and HUD using the new UI.
The ClienHUD and ServerHuD only contain whats necessary to manage/start a server or a client.(none of the matchmaking parts).
The CustumNetworkManager is where you setup and manage the server, and where the direct Server to Client messages are Registered, Send and Recieved.
This is also where the connection info is monitored.(OnServerConnect(), OnServerAddPlayer(), OnClientConnect()... etcetera).
You can of course add or remove anything you want to them as they work exactely the same as the standard Unity Components.

The Main Menu scene is setup to load the Server/Client scene where the ServerHUD or the ClientHUD is setup.(enables the serverHUD or clientHUd script).
From there you can manage/fill in the Server or Client connection info and start the server or connect to the server and load the next scene, in this case it goes to a Lobby/Game scene.
This scene would be where you build your Lobby or your Game if you want players to jump right into it.

In this scene a "Dummy Player" is spawned by the NetworkManager, this could be your actual Player a Lobby Player or just an "Empty" object
that handles player spawning. 

This package is just a Server and a Client setup in one build that lets you connect and transition into a Lobby or a Game Scene.

For any Questions, Requests or Comments please contact me at: 
PaulosCreations@gmail.com

If you find this Package usefull, Rating it or leaving a review is allways appreciated.