using MultiplayerExtensions.Environment;
using MultiplayerExtensions.Patchers;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    public class MpexLocalActivePlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // stuff needed for solo environments to work
            Container.BindInterfacesAndSelfTo<MpexLevelEndActions>().AsSingle();
            Container.Bind<EnvironmentContext>().FromInstance(EnvironmentContext.Gameplay).AsSingle();

            // other stuff
            Container.BindInterfacesAndSelfTo<LagReducerPatcher>().AsSingle();
        }
    }
}
