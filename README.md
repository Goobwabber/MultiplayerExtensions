# MultiplayerExtensions (PC Only) [![Build](https://github.com/Zingabopp/MultiplayerExtensions/workflows/Build/badge.svg?event=push)](https://github.com/Zingabopp/MultiplayerExtensions/actions?query=workflow%3ABuild)
A Beat Saber mod that expands Beat Saber's multiplayer functionality. **This is a work in progress which has bugs.**

## Features
* Allows custom levels to be selected in private lobbies.
* Attempts to download missing songs from Beat Saver.
* Warns the user when a song is not on Beat Saver.
* Adds HUD configuration options.
* Kicks unmodded players when custom levels are enabled.

## Installation
MultiplayerExtensions has not been released yet, but you can grab the latest build which is automagically generated. 
1. Download the `MultiplayerExtensions` file listed under `Artifacts` **[Here](https://github.com/Zingabopp/MultiplayerExtensions/actions?query=workflow%3ABuild+branch%3Amaster)** (pick the topmost successful build). 
   * You must be logged into GitHub to download builds from GitHub Actions.
2. Extract the zip file to your Beat Saber game directory (the one `Beat Saber.exe` is in).
   * The `MultiplayerExtensions.dll` (and `MultiplayerExtensions.pdb` if it exists) should end up in your `Plugins` folder (**NOT** the one in `Beat Saber_Data`).
   
## Requirements
These can be downloaded from [BeatMods](https://beatmods.com/#/mods) or using Mod Assistant. **Do NOT use any of the DLLs in the `Refs` folder, they have been stripped of code and will not work.**
* SongCore v3.0.0+
* BeatSaverSharp v1.6.0+
* BeatSaberMarkupLanguage v1.4.0+

## Reporting Issues
* The best way to report issues is to click on the `Issues` tab at the top of the GitHub page. This allows any contributor to see the problem and attempt to fix it, and others with the same issue can contribute more information.
* Include in your issue:
  * A detailed explanation of your problem (you can also attach videos/screenshots)
  * **Important**: The log file from the game session the issue occurred (restarting the game creates a new log file).
    * The log file can be found at `Beat Saber\Logs\_latest.log` (`Beat Saber` being the folder `Beat Saber.exe` is in).
* If you ask for help on Discord, at least include your `_latest.log` file in your help request.

## Contributing
Anyone can feel free to contribute bug fixes or enhancements to MultiplayerExtensions. Please keep in mind that this mod's purpose is to expand the functionality of official multiplayer, so we will likely not be accepting enhancements that require 3rd party servers. GitHub Actions for Pull Requests made from GitHub accounts that don't have direct access to the repository will fail. This is normal because the Action requires a `Secret` to download dependencies.
### Building
Visual Studio 2019 with the [BeatSaberModdingTools](https://github.com/Zingabopp/BeatSaberModdingTools) extension is the recommended development environment.
1. Check out the repository
2. Open `MultiplayerExtensions.sln`
3. Right-click the `MultiplayerExtensions` project, go to `Beat Saber Modding Tools` -> `Set Beat Saber Directory`
   * This assumes you have already set the directory for your Beat Saber game folder in `Extensions` -> `Beat Saber Modding Tools` -> `Settings...`
   * If you do not have the BeatSaberModdingTools extension, you will need to manually create a `MultiplayerExtensions.csproj.user` file to set the location of your game install. An example is showing below.
4. The project should now build.

**Example csproj.user File:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <BeatSaberDir>Full\Path\To\Beat Saber</BeatSaberDir>
  </PropertyGroup>
</Project>
```
## Related Mods
* [BeatSaberServerBrowser](https://github.com/roydejong/BeatSaberServerBrowser)
