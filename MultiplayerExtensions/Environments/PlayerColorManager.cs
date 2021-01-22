using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class PlayerColorManager : IInitializable, IDisposable
    {
        protected readonly PacketManager _packetManager;
        protected readonly IMultiplayerSessionManager _sessionManager;
        protected readonly LobbyPlaceManager _placeManager;

        internal PlayerColorManager(PacketManager packetManager, IMultiplayerSessionManager sessionManager, LobbyPlaceManager placeManager)
        {
            _packetManager = packetManager;
            _sessionManager = sessionManager;
            _placeManager = placeManager;
        }

        public void Initialize()
        {
            MPEvents.LobbyEnvironmentLoaded += HandleLobbyEnvironmentLoaded;
            _sessionManager.playerConnectedEvent += HandlePlayerConnected;
            _packetManager.RegisterCallback<PlayerColorPacket>(HandlePlayerColorPacket);
        }

        public void Dispose()
        {
            MPEvents.LobbyEnvironmentLoaded -= HandleLobbyEnvironmentLoaded;
            _sessionManager.playerConnectedEvent -= HandlePlayerConnected;
            _packetManager.UnregisterCallback<PlayerColorPacket>();
        }

        private void HandleLobbyEnvironmentLoaded(object sender, System.EventArgs e)
        {
            _placeManager.SetAllPlayerPlaceColor(Color.black);
            _packetManager.Send(new PlayerColorPacket() { color = Plugin.Config.Color });
            Color color;
            if (ColorUtility.TryParseHtmlString(Plugin.Config.Color, out color))
                _placeManager.SetPlayerPlaceColor(_sessionManager.localPlayer, color);
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
            => _packetManager.Send(new PlayerColorPacket() { color = Plugin.Config.Color });

        private void HandlePlayerColorPacket(PlayerColorPacket packet, IConnectedPlayer player)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(packet.color, out color))
                _placeManager.SetPlayerPlaceColor(player, color);
        }
    }
}
