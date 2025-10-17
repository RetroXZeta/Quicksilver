# Quicksilver

An incredibly basic TCP chat client for Windows, built in C# with the graphics library [SFML](https://www.sfml-dev.org/).

## Introduction

Quicksilver is a very basic TCP chat client intended mainly as a way to teach myself some basic networking.

I first played around with creating similar software several months ago, and recently decided to remake the project in an actually presentable manner. So, here it is!

## Usage

When running an instance of the application, the user can select to host or join a chatroom.

While hosting, currently, the application has no GUI, but don't worry! It's still working in the background.

To join the server being hosted on your network, open another instance of the application and join your server with the IP 127.0.0.1 (a.k.a. the localhost).

To join a server hosted on another network, you'll need that network's public IP (NOTE: you need to have a port forwarded for this). The public IP is displayed to the host when creating a server.

## Features

Quicksilver includes some special features. Text can be formatted with in-line text commands, with the format `{command:arg0,arg1,arg2,...}`.

### List of In-Line Commands

- `{bold}` - Toggles bold text.
- `{bold:true/false}` - Turns bold text on or off manually.
- `{color:color}` - Sets the color of the text. Currently accepted color arguments are `white`, `red`, `green`, `blue`, `yellow`, `cyan`, `magenta`, and `black`. TODO: Add RGB Hex color support.
- `{wave:amplitude,frequency,phase}` - Gives text a wavy effect. The amplitude is measured in pixels, the frequency is measured as the duration in seconds of a wave for a single character, and the phase denotes the difference in phase between consecutive characters.
- `{glyph:glyph}` - Inserts a custom image into the text. This image is referenced by its name in the Glyphs folder, and must exist in that folder for all users to be rendered by all users. For example, assuming all users have the default Glyphs folder, `{glyph:thumbsup}` will generate a thumbs up glyph. Custom glyphs can be added to the Glyphs folder.
