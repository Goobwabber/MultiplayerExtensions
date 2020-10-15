# Beat Saber Plugin Build Tools
### `CollectDependencies`

This allows you to specify any required files from the Beat Saber installation directory. 
It expects a directory `Refs` in your solution root, with a file `refs.txt` in the following format:

- Levels are indented using double quotes. Each additional leaf adds an entry to the represented list by simply concatenating all of its parents together to get a path.
- Any entry where the filename begins with `::` is treated as a special command. Currently there are 2 supported:
  1. `from` - reads from a file relative to the solution root. It does not do any handling of BOMs or trailing newlines. (those will cause the collector to break)
  2. `prompt` - prompts stdin for the node value. Not particularly useful, but there.
- At the end of any *full entry* there may be a question mark followed by a word. This word changes how the files are copied.
  1. `virt` - If the file is a DLL, virtualizes every type in the DLL like how IPA does normally.
  2. `native` - Forces the file to be copied without modification.
  3. *nothing* - By default, if the file is a DLL, all method bodies are removed to prevent copyright violations. It still behaves fine when linking.

It is recommended to have a `bsinstalldir.txt` in your solution root so that your `refs.txt` is not dependent on one machine's configuration.
An example `refs.txt` for the SongLoader follows:
```
::from ./bsinstalldir.txt
"Beat Saber_Data/
""Managed/
"""Assembly-CSharp
"""".dll?virt
""""-firstpass.dll
"""IllusionInjector.dll
"""IllusionPlugin.
""""dll
""""xml
"""UnityEngine.
""""dll
""""CoreModule.
"""""dll
"""""xml
""""AudioModule.
"""""dll
"""""xml
""""ImageConversionModule.
"""""dll
"""""xml
""""JSONSerializeModule.
"""""dll
"""""xml
""""UI.
"""""dll
"""""xml
""""UIElementsModule.
"""""dll
"""""xml
""""UIModule.
"""""dll
"""""xml
""""UnityWebRequestModule.
"""""dll
"""""xml
""""UnityWebRequestWWWModule.
"""""dll
"""""xml
"""TextMeshPro-1.0.55.2017.1.0b12.dll
"Plugins/
""BeatSaberCustomUI.dll
""CustomPlatforms.dll
```

In this example, `bsinstalldir.txt` must contain the full path to the root installation directory of Beat Saber, using forward slashes, with a trailing slash.

### `AssemblyRenameStep`

This defines an MSBuild target to automatically rename assemblies based on their version, according to [BSIPA's `Libs/` requirements](https://github.com/beat-saber-modding-group/BeatSaber-IPA-Reloaded/wiki/Developing#additional-libraries).
Usage can be seen in BSIPA's [`IPA.Injector/PostBuild.msbuild`](https://github.com/beat-saber-modding-group/BeatSaber-IPA-Reloaded/blob/master/IPA.Injector/PostBuild.msbuild).
