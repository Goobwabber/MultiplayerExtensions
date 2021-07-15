# MultiplayerExtensions ![Steam/PC-Only](https://goobi.moe/badges/api/v2/text?text=Steam/PC-Only&widthPadding=-80) [![Build](https://github.com/Zingabopp/MultiplayerExtensions/workflows/Build/badge.svg?event=push)](https://github.com/Zingabopp/MultiplayerExtensions/actions?query=workflow%3ABuild+branch%3Amaster)
A Beat Saber mod that expands Beat Saber's multiplayer functionality. **This is a work in progress which has bugs.**

## Features
* Allows custom levels to be selected in private lobbies.
* Attempts to download missing songs from Beat Saver.
* Warns the user when a song is not on Beat Saver.
* Adds HUD configuration options.
* Kicks unmodded players when custom levels are enabled.
* 10 player lobbies __(BeatTogether Exclusive feature)__
* [Cool stats page](https://mpex.goobwabber.com)

## Installation
1. Ensure you have the [required mods](https://github.com/Goobwabber/MultiplayerExtensions#requirements).
2. Download the `MultiplayerExtensions` file listed under `Assets` **[Here](https://github.com/Goobwabber/MultiplayerExtensions/releases)**.
   * Optionally, you can get a development build by downloading the file listed under `Artifacts`  **[Here](https://github.com/Goobwabber/MultiplayerExtensions/actions?query=workflow%3ABuild+branch%3Amaster)** (pick the topmost successful build).
   * You must be logged into GitHub to download a development build.
3. Extract the zip file to your Beat Saber game directory (the one `Beat Saber.exe` is in).
   * The `MultiplayerExtensions.dll` (and `MultiplayerExtensions.pdb` if it exists) should end up in your `Plugins` folder (**NOT** the one in `Beat Saber_Data`).
4. **Optional**: Edit `Beat Saber IPA.json` (in your `UserData` folder) and change `Debug` -> `ShowCallSource` to `true`. This will enable BSIPA to get file and line numbers from the `PDB` file where errors occur, which is very useful when reading the log files. This may have a *slight* impact on performance.

Lastly, check out [other mods](https://github.com/Goobwabber/MultiplayerExtensions#related-mods) that work well with MultiplayerExtensions!

## Requirements
These can be downloaded from [BeatMods](https://beatmods.com/#/mods) or using Mod Assistant. **Do NOT use any of the DLLs in the `Refs` folder, they have been stripped of code and will not work.**
* SongCore v3.0.3+
* BeatSaverSharp v2.0.1+
* BeatSaberMarkupLanguage v1.4.5+
* SiraUtil 2.4.0+
* BeatTogether 1.1.0+ (Only required for 10 player lobbies)

## Troubleshooting
#### Custom Songs button not appearing
* Most of the time this is because you didn't install MultiplayerExtensions correctly, or are missing a required dependency (most likely SiraUtil or BeatSaverSharp).
  * Open `Beat Saber\Logs\_latest.log` and search the text for `MultiplayerExtensions`
  * If you see a line like `[WARNING @ 19:20:44 | IPA/Loader] MultiplayerExtensions is missing dependency BeatSaverSharp@^1.6.0`, that's what you need to fix.
  * In your log, there's a section where IPA lists loaded plugins. Check and make sure MultiplayerExtensions is listed there.
  ```
  [INFO @ 12:41:52 | IPA] Beat Saber
  [INFO @ 12:41:52 | IPA] Running on Unity 2019.3.15f1
  [INFO @ 12:41:52 | IPA] Game version 1.13.0
  [INFO @ 12:41:52 | IPA] -----------------------------
  [INFO @ 12:41:52 | IPA] Loading plugins from Plugins and found 25
  [INFO @ 12:41:52 | IPA] -----------------------------
  [INFO @ 12:41:52 | IPA] Beat Saber IPA (BSIPA): 4.1.3
  [INFO @ 12:41:52 | IPA] SiraUtil (SiraUtil): 2.1.0
  [INFO @ 12:41:52 | IPA] INI Parser (Ini Parser): 2.5.7
  [INFO @ 12:41:52 | IPA] BS_Utils (BS Utils): 1.6.3
  [INFO @ 12:41:52 | IPA] BeatSaberMarkupLanguage (BeatSaberMarkupLanguage): 1.4.1
  [INFO @ 12:41:52 | IPA] SongCore (SongCore): 3.0.2
  [INFO @ 12:41:52 | IPA] BeatSaverSharp (BeatSaverSharp): 1.6.0
  [INFO @ 12:41:52 | IPA] MultiplayerExtensions (MultiplayerExtensions): 0.2.0
  [INFO @ 12:41:52 | IPA] -----------------------------
  [INFO @ 12:41:52 | IPA] -----------------------------```
* If MultiplayerExtensions is loading and the Custom Songs button isn't showing up, you may need to do a fresh install of Beat Saber.

## Reporting Issues
* The best way to report issues is to click on the `Issues` tab at the top of the GitHub page. This allows any contributor to see the problem and attempt to fix it, and others with the same issue can contribute more information. **Please try the troubleshooting steps before reporting the issues listed there. Please only report issues after using the latest build, your problem may have already been fixed.**
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
## Donate
You can support development of MultiplayerExtensions by donating at the following links:
* https://www.patreon.com/goobwabber
* https://ko-fi.com/goobwabber
* https://ko-fi.com/zingabopp

## Related Mods
* [BeatSaberServerBrowser](https://github.com/roydejong/BeatSaberServerBrowser)
* [MultiplayerAvatars](https://github.com/Goobwabber/MultiplayerAvatars)
* BeatTogether for [PC](https://github.com/pythonology/BeatTogether) or [Quest](https://github.com/pythonology/BeatTogether.Quest)
---
[![donate](https://goobi.moe/badges/api/v2/donate?text=Donate!&scale=1.5&fontsize=32&radius=8&textXOffset=5&height=12.9666&widthOffset=3.5666)](https://github.com/Goobwabber/MultiplayerExtensions#donate) [![mpex stats](https://goobi.moe/badges/api/v2/mpexusercount?scale=1.5&radius=8&textXOffset=-95&height=35&textanchor=begin&widthOffset=-20.3466&textYOffset=-4&innerSpacing=5)](https://mpex.goobwabber.com)
