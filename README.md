# MultiplayerExtensions (PC Only) [![Build](https://github.com/Zingabopp/MultiplayerExtensions/workflows/Build/badge.svg?event=push)](https://github.com/Zingabopp/MultiplayerExtensions/actions?query=workflow%3ABuild)
A Beat Saber mod that expands multiplayer functionality. **This is a work in progress which has bugs.**

## Features
* Allows custom levels to be selected in private lobbies.
* Attempts to download missing songs from Beat Saver.
* Warns the user when a song is not on Beat Saver.
* Adds HUD configuration options.
* Kicks unmodded players when custom levels are enabled.

## Installation
MultiplayerExtensions has not been released yet, but you can grab the latest build which is automagically generated. 
1. Download `MultiplayerExtensions` file listed under `Artifacts` [Here](https://github.com/Zingabopp/MultiplayerExtensions/actions?query=workflow%3ABuild+branch%3Amaster) (pick the topmost successful build). 
   * You must be logged into GitHub to download builds from GitHub Actions.
2. Extract the zip file to your Beat Saber game directory.
   * The `MultiplayerExtensions.dll` (and `MultiplayerExtensions.pdb` if it exists) should end up in your `Plugins` folder.

## Requirements
These can be downloaded from [BeatMods](https://beatmods.com/#/mods) or using Mod Assistant. **Do NOT use any of the DLLs in the `Refs` folder, they will not work**
* SongCore v3.0.0+
* BeatSaverSharp v1.6.0+
* BeatSaberMarkupLanguage v1.4.0+

## Related Mods
* [BeatSaberServerBrowser](https://github.com/roydejong/BeatSaberServerBrowser)
