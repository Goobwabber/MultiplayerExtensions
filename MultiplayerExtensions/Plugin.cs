using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Installers;
using MultiplayerExtensions.UI;
using SiraUtil.Zenject;
using MultiplayerExtensions.Utilities;
using System;
using System.Reflection;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace MultiplayerExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string HarmonyId = "com.github.Zingabopp.MultiplayerExtensions";
        internal static Plugin Instance { get; private set; } = null!;
        internal static PluginMetadata PluginMetadata = null!;
        internal static Harmony? _harmony;
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
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Plugin.Log?.Info($"MultiplayerExtensions: '{VersionInfo.Description}'");
            HarmonyManager.ApplyDefaultPatches();
            Task versionTask = CheckVersion();

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
                if(PluginMetadata != null)
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
            catch(ReleaseNotFoundException ex)
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
