# XChat
A simple clientless chat emulator for rappelz.

-> Networking class taken and adapted from the Rappelz: Endless Odyssey project by NCarbon.
-> Special thanks to glandu2 & his rzauth to get an understanding about the Login encryption

# How to use
First of all, do not take this project as a real program, because it isn't.
It was initially created for myself to get into the game without having to run the client, because I can't run Rappelz when I'm in University, that'd be a bit odd (also, my resolution on my Netbook sucks, so I wasn't able to get Rappelz displayed in window mode properly).

Secondly, and this is for you, I created this project as a challenge to the game server: "What do I need to let the game server think that I'm a real client?" - The answer is: Basically nothing. I am literally able to ignore every packet the game server sends as long as I send a "ping"-packet to keep the connection alive. That actually means you can use the base of this project for many things, as example coding a clientless farming bot.

I based the project on C# for 2 reasons:
-> 1. Most people which are able to code are able to understand C#.
-> 2. I needed a GUI. And what's the quickest way to create an UI? Guess what.

tl;dr:
Use this project to get an understanding about the networking of Rappelz.

Kind Regards,

Xijezu
