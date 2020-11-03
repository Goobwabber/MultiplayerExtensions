using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.UI;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace MultiplayerExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string HarmonyId = "com.github.Zingabopp.MultiplayerExtensions";
        internal static Plugin Instance { get; private set; } = null!;
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
        public Plugin(IPALogger logger, Config conf)
        {
            Instance = this;
            Log = logger;
            Config = conf.Generated<PluginConfig>();
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("Multiplayer", "MultiplayerExtensions.UI.GameplaySetupPanel.bsml", GameplaySetupPanel.instance);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Plugin.Log?.Info($"MultiplayerExtensions: '{VersionInfo.Description}'");
            HarmonyManager.ApplyDefaultPatches();
        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }
    }
}
