using MultiplayerExtensions.Packets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    class PlayerColorManager : IInitializable
    {
        [Inject]
        protected PacketManager packetManager;

        [Inject]
        protected IMultiplayerSessionManager multiplayerSessionManager;

        [Inject]
        protected LobbyPlaceManager placeManager;

        public void Initialize()
        {
            MPEvents.LobbyEnvironmentLoaded += HandleLobbyEnvironmentLoaded;
            multiplayerSessionManager.playerConnectedEvent += HandlePlayerConnected;
            packetManager.RegisterCallback<PlayerColorPacket>(HandlePlayerColorPacket);
        }

        private void HandleLobbyEnvironmentLoaded(object sender, System.EventArgs e)
        {
            packetManager.Send(new PlayerColorPacket() { color = Plugin.Config.Color });
            Color color;
            if (ColorUtility.TryParseHtmlString(Plugin.Config.Color, out color))
                placeManager.SetPlayerPlaceColor(multiplayerSessionManager.localPlayer, color);
        }

        public void HandlePlayerConnected(IConnectedPlayer player)
            => packetManager.Send(new PlayerColorPacket() { color = Plugin.Config.Color });

        public void HandlePlayerColorPacket(PlayerColorPacket packet, IConnectedPlayer player)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(packet.color, out color))
                placeManager.SetPlayerPlaceColor(player, color);
        }

        public class Team
        {
            public string name;
            public Color color;

            public Team(string name, string color)
            {
                this.name = name;
                ColorUtility.TryParseHtmlString(color, out this.color);
            }
        }
    }
}
