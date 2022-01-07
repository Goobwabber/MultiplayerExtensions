using MultiplayerCore.Networking;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Players
{
	public class MpexPlayerManager : IInitializable
	{
		public IReadOnlyDictionary<string, MpexPlayer> Players => _playerData;

		private ConcurrentDictionary<string, MpexPlayer> _playerData = new();

		private readonly MpPacketSerializer _packetSerializer;
		private readonly IMultiplayerSessionManager _sessionManager;
		private readonly Config _config;

		internal MpexPlayerManager(
			MpPacketSerializer packetSerializer,
			IMultiplayerSessionManager sessionManager,
			Config config)
		{
			_packetSerializer = packetSerializer;
			_sessionManager = sessionManager;
			_config = config;
		}

		public void Initialize()
		{
			_sessionManager.SetLocalPlayerState("modded", true);
			_packetSerializer.RegisterCallback<MpexPlayer>(HandlePlayerData);
			_sessionManager.playerConnectedEvent += HandlePlayerConnected;
		}

		public void Dispose()
		{
			_packetSerializer.UnregisterCallback<MpexPlayer>();
		}

		private void HandlePlayerConnected(IConnectedPlayer player)
		{
			if (ColorUtility.TryParseHtmlString(_config.Color, out Color color))
				_sessionManager.Send(new MpexPlayer
				{
					Color = color
				});
		}

		private void HandlePlayerData(MpexPlayer packet, IConnectedPlayer player)
		{
			_playerData[player.userId] = packet;
		}

		public bool TryGetPlayer(string userId, out MpexPlayer player)
			=> _playerData.TryGetValue(userId, out player);
    }
}
