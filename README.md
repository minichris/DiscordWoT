# DiscordWoT
A discord bot that shows player and tank stats.
##How to install (Linux)
With Dotnet installed, run:
```sh
git clone https://github.com/minichris/DiscordWoT.git
```
I then suggest you create a script like the following:
```sh
#!/bin/sh
cd DiscordWoT && git checkout master && git up
killall dotnet
nohup dotnet run -p DiscordWoTCore &>/dev/null &
```
This can be called to start the bot, and by crontab to keep the files up to date.

You must make a file called "Token.txt" in the root folder with the first line being the Discord Bot Token, and the second being the Wargamming API key, which is the ID of the Wargamming application.
