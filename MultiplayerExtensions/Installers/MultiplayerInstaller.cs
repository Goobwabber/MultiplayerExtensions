using MultiplayerExtensions.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class MultiplayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.Log?.Info("Injecting Dependencies");
            Container.BindInterfacesAndSelfTo<ExtendedSessionManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ExtendedPlayerManager>().AsSingle();
        }
    }
}
