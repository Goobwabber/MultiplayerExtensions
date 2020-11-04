using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Networking
{
    class ExtendedPlayerManager : IInitializable
    {
        [Inject]
        private ExtendedSessionManager _sessionManager;

        public string localPlatformID;

        public void Initialize()
        {
            _sessionManager.playerConnectedEvent += OnPlayerConnected;
            _sessionManager.RegisterCallback(ExtendedSessionManager.MessageType.PlayerUpdate, HandlePlayerPacket, new Func<ExtendedPlayerPacket>(ExtendedPlayerPacket.pool.Obtain));

            BS_Utils.Gameplay.GetUserInfo.GetUserAsync().ContinueWith(r =>
            {
                localPlatformID = r.Result.platformUserId;
            });
        }

        private void OnPlayerConnected(ExtendedPlayer player)
        {
            if (localPlatformID != null)
            {
                ExtendedPlayerPacket localPlayerPacket = new ExtendedPlayerPacket().Init(localPlatformID);
                _sessionManager.Send(localPlayerPacket);
            }
        }

        private void HandlePlayerPacket(ExtendedPlayerPacket packet, ExtendedPlayer player)
        {
            player.platformID = packet.platformID;
        }
    }
}
