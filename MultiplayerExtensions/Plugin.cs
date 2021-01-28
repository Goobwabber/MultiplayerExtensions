using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Installers;
using SiraUtil.Zenject;
using MultiplayerExtensions.Utilities;
using System;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;
using BeatSaverSharp;
using System.Diagnostics;

namespace MultiplayerExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string HarmonyId = "com.github.Zingabopp.MultiplayerExtensions";
        internal static Plugin Instance { get; private set; } = null!;
        internal static PluginMetadata PluginMetadata = null!;
        internal static Harmony? _harmony;
        internal static BeatSaver BeatSaver = null!;
        internal static Harmony Harmony
        {
            get
            {
                return _harmony ??= new Harmony(HarmonyId);
            }
        }
        /// <summary>
        /// Use to send log messages through BSIPA.
        /// </summary>
        internal static IPALogger Log { get; private set; } = null!;
        internal static PluginConfig Config = null!;

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            Instance = this;
            PluginMetadata = pluginMetadata;
            Log = logger;
            Config = conf.Generated<PluginConfig>();
            zenjector.OnApp<MultiplayerInstaller>();
            zenjector.OnMenu<InterfaceInstaller>();
            HttpOptions options = new HttpOptions("MultiplayerExtensions", new Version(pluginMetadata.Version.ToString()));
            BeatSaver = new BeatSaver(options);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Plugin.Log?.Info($"MultiplayerExtensions: '{VersionInfo.Description}'");

            if (Plugin.Config.MaxPlayers > 100)
                Plugin.Config.MaxPlayers = 100;
            if (Plugin.Config.MaxPlayers < 10)
                Plugin.Config.MaxPlayers = 10;

            HarmonyManager.ApplyDefaultPatches();
            Task versionTask = CheckVersion();
            MPEvents_Test();
        }

        [Conditional("DEBUG")]
        public void MPEvents_Test()
        {
            MPEvents.BeatmapSelected += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.LevelId))
                    Log?.Warn($"BeatmapSelected by '{e.UserId}|{e.UserType.ToString()}': {e.LevelId}|{e.BeatmapDifficulty}|{e.BeatmapCharacteristic?.name ?? "<NULL>"}");
                else
                    Log?.Warn($"Beatmap Cleared by '{e.UserId}|{e.UserType.ToString()}'");
            };
        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }

        public async Task CheckVersion()
        {
            try
            {
                GithubVersion latest = await VersionCheck.GetLatestVersionAsync("Zingabopp", "MultiplayerExtensions");
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
