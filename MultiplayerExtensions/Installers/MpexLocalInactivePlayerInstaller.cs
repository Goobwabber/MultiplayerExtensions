using MultiplayerExtensions.Patchers;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    public class MpexLocalInactivePlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LagReducerPatcher>().AsSingle();
        }
    }
}
