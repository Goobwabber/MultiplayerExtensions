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
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private readonly Harmony _harmony;
        private readonly PluginMetadata _metadata;
        public const string ID = "com.goobwabber.multiplayerextensions";

        [Init]
        public Plugin(IPALogger logger, Conf conf, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            Config config = conf.Generated<Config>();

            _harmony = new Harmony(ID);
            _metadata = pluginMetadata;

            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseSiraSync(SiraUtil.Web.SiraSync.SiraSyncServiceType.GitHub, "Goobwabber", "MultiplayerExtensions");
            zenjector.Install<MpexAppInstaller>(Location.App, config);
            zenjector.Install<MpexMenuInstaller>(Location.Menu);
            zenjector.Install<MpexLobbyInstaller, MultiplayerLobbyInstaller>();
            zenjector.Install<MpexGameInstaller>(Location.MultiplayerCore);
            zenjector.Install<MpexLocalActivePlayerInstaller, MultiplayerLocalActivePlayerInstaller>();
            zenjector.Install<MpexLocalInactivePlayerInstaller, MultiplayerLocalInactivePlayerInstaller>();
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
