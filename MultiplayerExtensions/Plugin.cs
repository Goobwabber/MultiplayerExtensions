using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using MultiplayerExtensions.Installers;
using SiraUtil.Zenject;
using MultiplayerExtensions.Utilities;
using System;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;
using BeatSaverSharp;
using System.Diagnostics;
using MultiplayerExtensions.UI;
using BeatSaberMarkupLanguage.Settings;

namespace MultiplayerExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string HarmonyId = "com.github.Goobwabber.MultiplayerExtensions";

        internal static Plugin Instance { get; private set; } = null!;
        internal static PluginMetadata PluginMetadata = null!;

        internal static bool IsNoodleInstalled { get; private set; }
        internal static bool IsMappingInstalled { get; private set; }
        internal static bool IsChromaInstalled { get; private set; }

        internal static Hive.Versioning.Version ProtocolVersion { get; } = new Hive.Versioning.Version("0.7.1");
        internal static IPALogger Log { get; private set; } = null!;
        internal static PluginConfig Config = null!;

        internal static BeatSaver BeatSaver = null!;
        internal static Harmony? _harmony;
        internal static Harmony Harmony
        {
            get
            {
                return _harmony ??= new Harmony(HarmonyId);
            }
        }


        private const int MaxPlayers = 100;
        private const int MinPlayers = 10;

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            Instance = this;
            PluginMetadata = pluginMetadata;
            Log = logger;
            Config = conf.Generated<PluginConfig>();

            zenjector.OnApp<MPCoreInstaller>();
            zenjector.OnMenu<MPMenuInstaller>();
            zenjector.OnGame<MPGameInstaller>().OnlyForMultiplayer();

            BeatSaverOptions options = new BeatSaverOptions("MultiplayerExtensions", new Version(pluginMetadata.Version.ToString()));
            options.Timeout = TimeSpan.FromMinutes(1);
            BeatSaver = new BeatSaver(options);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Plugin.Log?.Info($"MultiplayerExtensions: '{VersionInfo.Description}'");
            BSMLSettings.instance.AddSettingsMenu("Multiplayer", "MultiplayerExtensions.UI.settings.bsml", MPSettings.instance);

            Plugin.Config.MaxPlayers = Math.Max(Math.Min(Config.MaxPlayers, MaxPlayers), MinPlayers);

            HarmonyManager.ApplyDefaultPatches();
            Task versionTask = CheckVersion();

            IsNoodleInstalled = IPA.Loader.PluginManager.IsEnabled(IPA.Loader.PluginManager.GetPluginFromId("NoodleExtensions"));
            IsMappingInstalled = IPA.Loader.PluginManager.IsEnabled(IPA.Loader.PluginManager.GetPluginFromId("MappingExtensions"));
            IsChromaInstalled = IPA.Loader.PluginManager.IsEnabled(IPA.Loader.PluginManager.GetPluginFromId("Chroma"));

            MPEvents_Test();
            
            Sprites.PreloadSprites();
        }

        [Conditional("DEBUG")]
        public void MPEvents_Test()
        {
            MPEvents.BeatmapSelected += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.LevelId))
                    Log?.Warn($"Beatmap Selected by '{e.UserId}|{e.UserType.ToString()}': {e.LevelId}|{e.BeatmapDifficulty}|{e.BeatmapCharacteristic?.name ?? "<NULL>"}");
                else
                    Log?.Warn($"Beatmap Selection Cleared by '{e.UserId}|{e.UserType.ToString()}'");
            };
        }
        [Conditional("DEBUG")]
        public static void DebugLog(string s) => Log.Debug(s);
        [Conditional("DEBUG")]
        public static void DebugLog(Exception ex) => Log.Debug(ex);

        [OnExit]
        public void OnApplicationQuit()
        {

        }

        public async Task CheckVersion()
        {
            try
            {
                GithubVersion latest = await VersionCheck.GetLatestVersionAsync("Goobwabber", "MultiplayerExtensions");
                Log?.Debug($"Latest version is {latest}, released on {latest.ReleaseDate.ToShortDateString()}");
                if (PluginMetadata != null)
                {
                    SemVer.Version currentVer = PluginMetadata.Version;
                    SemVer.Version latestVersion = new SemVer.Version(latest.ToString());
                    bool updateAvailable = new SemVer.Range($">{currentVer}").IsSatisfied(latestVersion);
                    if (updateAvailable)
                    {
                        Log?.Info($"An update is available!\nNew mod version: {latestVersion}\nCurrent mod version: {currentVer}");
                    }
                }
            }
            catch (ReleaseNotFoundException ex)
            {
                Log?.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                Log?.Warn($"Error checking latest version: {ex.Message}");
                Log?.Debug(ex);
            }
        }
    }
}
