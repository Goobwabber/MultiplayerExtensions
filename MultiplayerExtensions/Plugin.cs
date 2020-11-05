using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using MultiplayerExtensions.HarmonyPatches;
using MultiplayerExtensions.Installers;
using MultiplayerExtensions.UI;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;
using MultiplayerExtensions.Downloaders;
using MultiplayerExtensions.Avatars;
using System.Reflection;

namespace MultiplayerExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string HarmonyId = "com.github.Zingabopp.MultiplayerExtensions";
        public static readonly string UserAgent = $"MultiplayerExtensions/{Assembly.GetExecutingAssembly().GetName().Version} {VersionInfo.Description}";
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
        internal static IAvatarProvider? AvatarProvider = null;
        internal static Zenjector Zenjector = null!;

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Config = conf.Generated<PluginConfig>();
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("Multiplayer", "MultiplayerExtensions.UI.GameplaySetupPanel.bsml", GameplaySetupPanel.instance);
            Zenjector = zenjector;
            Zenjector.OnApp<MultiplayerInstaller>();
            Plugin.Log?.Debug("Init finished.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Plugin.Log?.Info(UserAgent);
            HarmonyManager.ApplyDefaultPatches();
            Plugin.Log?.Debug("Installing bindings.");
        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }
    }
}
