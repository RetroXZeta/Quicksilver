# Quicksilver

An incredibly basic TCP chat client, built in C# with the graphics library [SFML](https://www.sfml-dev.org/).

## Introduction

Quicksilver is a very basic TCP chat client intended mainly as a way to teach myself some basic networking.

I first played around with creating similar software several months ago, and recently decided to remake the project in an actually presentable manner. So, here it is!

## Usage

When running an instance of the application, the user can select to host or join a chatroom.

While hosting, currently, the application has no GUI, but don't worry! It's still working in the background.

To join the server being hosted on your network, open another instance of the application and join your server with the IP 127.0.0.1 (a.k.a. the localhost).

To join a server hosted on another network, you'll need that network's public IP (NOTE: you need to have a port forwarded for this). The public IP is displayed to the host when creating a server.
