using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using MultiplayerExtensions.Installers;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;
using Conf = IPA.Config.Config;

namespace MultiplayerExtensions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public const string ID = "com.goobwabber.multiplayerextensions";

        internal static IPALogger Logger = null!;
        internal static Config Config = null!;

        private readonly Harmony _harmony;
        private readonly PluginMetadata _metadata;

        [Init]
        public Plugin(IPALogger logger, Conf conf, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            Config config = conf.Generated<Config>();
            _harmony = new Harmony(ID);
            _metadata = pluginMetadata;
            Logger = logger;
            Config = config;

            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseSiraSync(SiraUtil.Web.SiraSync.SiraSyncServiceType.GitHub, "Goobwabber", "MultiplayerExtensions");
            zenjector.Install<MpexAppInstaller>(Location.App, config);
            zenjector.Install<MpexMenuInstaller>(Location.Menu);
            zenjector.Install<MpexLobbyInstaller, MultiplayerLobbyInstaller>();
            zenjector.Install<MpexGameInstaller>(Location.MultiplayerCore);
            zenjector.Install<MpexLocalActivePlayerInstaller>(Location.MultiPlayer);
            zenjector.Install<MpexLocalInactivePlayerInstaller>(Location.InactiveMultiPlayer);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(_metadata.Assembly);
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
