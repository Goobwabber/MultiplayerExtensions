using MultiplayerCore.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Players
{
	public class MpexPlayerManager : IInitializable
	{
		public event Action<IConnectedPlayer, MpexPlayerData> PlayerConnectedEvent = null!;

		public IReadOnlyDictionary<string, MpexPlayerData> Players => _playerData;

		private ConcurrentDictionary<string, MpexPlayerData> _playerData = new();

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
			_packetSerializer.RegisterCallback<MpexPlayerData>(HandlePlayerData);
			_sessionManager.playerConnectedEvent += HandlePlayerConnected;
		}

		public void Dispose()
		{
			_packetSerializer.UnregisterCallback<MpexPlayerData>();
		}

		private void HandlePlayerConnected(IConnectedPlayer player)
		{
			_sessionManager.Send(new MpexPlayerData
			{
				Color = _config.PlayerColor
			});
		}

		private void HandlePlayerData(MpexPlayerData packet, IConnectedPlayer player)
		{
			_playerData[player.userId] = packet;
			PlayerConnectedEvent(player, packet);
		}

		public bool TryGetPlayer(string userId, out MpexPlayerData player)
			=> _playerData.TryGetValue(userId, out player);

		public MpexPlayerData? GetPlayer(string userId)
			=> _playerData.ContainsKey(userId) ? _playerData[userId] : null;
    }
}
