using MultiplayerExtensions.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Installers
{
    class InterfaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Plugin.Log?.Info("Installing Interface");

            HostLobbySetupViewController hostViewController = Container.Resolve<HostLobbySetupViewController>();
            HostLobbySetupPanel hostSetupPanel = hostViewController.gameObject.AddComponent<HostLobbySetupPanel>();
            Container.Inject(hostSetupPanel);

            ClientLobbySetupViewController clientViewController = Container.Resolve<ClientLobbySetupViewController>();
            ClientLobbySetupPanel clientSetupPanel = clientViewController.gameObject.AddComponent<ClientLobbySetupPanel>();
            Container.Inject(clientSetupPanel);
        }

        public override void Start()
        {
            CenterStageScreenController centerScreenController = Container.Resolve<CenterStageScreenController>();
            CenterScreenLoadingPanel loadingPanel = centerScreenController.gameObject.AddComponent<CenterScreenLoadingPanel>();
            Container.Inject(loadingPanel);
        }
    }
}
