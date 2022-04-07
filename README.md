# Modded MC Updater
A simple program to update your modded minecraft server (Modpack) if it uses the serverstarter.jar using the SSH.NET library.

I noticed more modpack devs are implementing the serverstarter.jar updater/installer into their pack for ease of installing/updating. I got lazy using ftp clients and running the jar manually, so I created this :)

# Instructions
Fill the hostname, port, username, password, Minecraft directory location and serverstarter.jar name in the source before compiling to work properly

Run the program, select the .yaml config file and click update

# TODO:
- Allow the user to set their hostname, port, username, password, Minecraft directory location and serverstarter.jar name in the program itself
- Show the update results in realtime instead of "freezing" and displaying the information after it's done
