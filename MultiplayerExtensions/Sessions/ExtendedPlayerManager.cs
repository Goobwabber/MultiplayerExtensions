using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Sessions
{
	public class ExtendedPlayerManager : IInitializable, IDisposable
	{
		protected readonly IMultiplayerSessionManager _sessionManager;
		protected readonly PacketManager _packetManager;
		protected readonly IPlatformUserModel _platformUserModel;

		private Dictionary<string, ExtendedPlayer> _players = new Dictionary<string, ExtendedPlayer>();
		internal string? localPlatformID;
		internal Platform localPlatform;
		internal Color localColor;

		public Dictionary<string, ExtendedPlayer> players { get => _players; }
		public event Action<ExtendedPlayer>? extendedPlayerConnectedEvent;

		public ExtendedPlayerManager(IMultiplayerSessionManager sessionManager, PacketManager packetManager, IPlatformUserModel platformUserModel)
		{
			_sessionManager = sessionManager;
			_packetManager = packetManager;
			_platformUserModel = platformUserModel;
		}

		public void Initialize()
		{
			Plugin.Log?.Info("Setting up PlayerManager");

			_sessionManager.playerConnectedEvent += OnPlayerConnected;
			_sessionManager.playerDisconnectedEvent += OnPlayerDisconnected;

			_packetManager.RegisterCallback<ExtendedPlayerPacket>(HandlePlayerPacket);

			if (!ColorUtility.TryParseHtmlString(Plugin.Config.Color, out localColor))
				localColor = new Color(0.031f, 0.752f, 1f);

			_platformUserModel.GetUserInfo().ContinueWith(r =>
			{
				localPlatformID = r.Result.platformUserId;
				localPlatform = r.Result.platform.ToPlatform();
			});
		}

		public void Dispose()
		{
			_sessionManager.playerConnectedEvent -= OnPlayerConnected;
			_sessionManager.playerDisconnectedEvent -= OnPlayerDisconnected;
			_packetManager.UnregisterCallback<ExtendedPlayerPacket>();
		}

		private void OnPlayerConnected(IConnectedPlayer player)
		{
			Plugin.Log?.Info($"Player '{player.userId}' joined");
			if (localPlatformID != null)
			{
				ExtendedPlayerPacket localPlayerPacket = new ExtendedPlayerPacket().Init(localPlatformID, localPlatform, localColor);
				_packetManager.Send(localPlayerPacket);
			}
		}

		private void OnPlayerDisconnected(IConnectedPlayer player)
		{
			Plugin.Log?.Info($"Player '{player.userId}' disconnected");
			// var extendedPlayer = _players[player.userId];
			_players.Remove(player.userId);
		}

		private void HandlePlayerPacket(ExtendedPlayerPacket packet, IConnectedPlayer player)
		{
			if (_players.ContainsKey(player.userId))
			{
				ExtendedPlayer extendedPlayer = _players[player.userId];
				extendedPlayer.platformID = packet.platformID;
				extendedPlayer.platform = packet.platform;
				extendedPlayer.playerColor = packet.playerColor;
				extendedPlayer.mpexVersion = new SemVer.Version(packet.mpexVersion);
			}
			else
            {
				Plugin.Log?.Info($"Received 'ExtendedPlayerPacket' from '{player.userId}' with platformID: '{packet.platformID}'  mpexVersion: '{packet.mpexVersion}'");
				ExtendedPlayer extendedPlayer = new ExtendedPlayer(player, packet.platformID, packet.platform, new SemVer.Version(packet.mpexVersion), packet.playerColor);

				if (Plugin.PluginMetadata.Version != extendedPlayer.mpexVersion)
				{
					Plugin.Log?.Warn("###################################################################");
					Plugin.Log?.Warn("Different MultiplayerExtensions version detected!");
					Plugin.Log?.Warn($"The player '{player.userName}' is using MultiplayerExtensions {extendedPlayer.mpexVersion} while you are using MultiplayerExtensions {Plugin.PluginMetadata.Version}");
					Plugin.Log?.Warn("For best compatibility all players should use the same version of MultiplayerExtensions.");
					Plugin.Log?.Warn("###################################################################");
				}

				_players[player.userId] = extendedPlayer;
				extendedPlayerConnectedEvent?.Invoke(extendedPlayer);
			}
		}

		public ExtendedPlayer? GetExtendedPlayer(IConnectedPlayer player)
			=> GetExtendedPlayer(player.userId);

		public ExtendedPlayer? GetExtendedPlayer(string userId)
        {
			if (_players.TryGetValue(userId, out ExtendedPlayer extendedPlayer))
				return extendedPlayer;
			return null;
		}
	}
}
