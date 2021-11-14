using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Extensions
{
	public class ExtendedSessionManager : MultiplayerSessionManager, IMultiplayerSessionManager
	{
		protected PacketManager _packetManager = null!;
		protected IPlatformUserModel _platformUserModel = null!;

		public IConnectedPlayer partyOwner { get; internal set; } = null!;

		private Dictionary<string, ExtendedPlayer> _extendedPlayers = new Dictionary<string, ExtendedPlayer>();
		public Dictionary<string, ExtendedPlayer> extendedPlayers { get => _extendedPlayers; }
		public static ExtendedPlayer localExtendedPlayer { get; protected set; } = null!;

		public event Action<ExtendedPlayer>? extendedPlayerConnectedEvent;

		[Inject]
		internal void Inject([InjectOptional] PacketManager packetManager, [InjectOptional] IPlatformUserModel platformUserModel)
		{
			_packetManager = packetManager;
			_platformUserModel = platformUserModel;
		}

		public new void StartSession(ConnectedPlayerManager connectedPlayerManager)
		{
			Plugin.Log?.Info("Setting up SessionManager");

			base.StartSession(connectedPlayerManager);

			localExtendedPlayer = new ExtendedPlayer(localPlayer);

			SetLocalPlayerState("modded", true);

			SetLocalPlayerState("ME_Installed", Plugin.IsMappingInstalled);
			SetLocalPlayerState("NE_Installed", Plugin.IsNoodleInstalled);
			SetLocalPlayerState("Chroma_Installed", Plugin.IsChromaInstalled);

			connectedEvent += HandleConnected;

			playerConnectedEvent += HandlePlayerConnected;
			playerDisconnectedEvent += HandlePlayerDisconnected;
			_packetManager.RegisterCallback<ExtendedPlayerPacket>(HandleExtendedPlayerPacket);

			if (!ColorUtility.TryParseHtmlString(Plugin.Config.Color, out localExtendedPlayer.playerColor))
				localExtendedPlayer.playerColor = new Color(0.031f, 0.752f, 1f);

			_platformUserModel.GetUserInfo().ContinueWith(r =>
			{
				localExtendedPlayer.platformID = r.Result.platformUserId;
				localExtendedPlayer.platform = r.Result.platform.ToPlatform();
			});
		}

		public new void EndSession()
		{
			playerConnectedEvent -= HandlePlayerConnected;
			playerDisconnectedEvent -= HandlePlayerDisconnected;
			_packetManager.UnregisterCallback<ExtendedPlayerPacket>();

			base.EndSession();
		}

		private void HandleConnected()
        {
			Plugin.Log?.Info($"Connected to lobby.");
        }

		private void HandlePlayerConnected(IConnectedPlayer player)
		{
			Plugin.Log?.Info($"Player '{player.userId}' joined");
			if (localExtendedPlayer.platformID != null)
			{
				ExtendedPlayerPacket localPlayerPacket = new ExtendedPlayerPacket().Init(localExtendedPlayer.platformID, localExtendedPlayer.platform, localExtendedPlayer.playerColor);
				_packetManager.Send(localPlayerPacket);
			}
		}

		private void HandlePlayerDisconnected(IConnectedPlayer player)
		{
			Plugin.Log?.Info($"Player '{player.userId}' disconnected");
			// var extendedPlayer = _players[player.userId];
			_extendedPlayers.Remove(player.userId);
		}

		private void HandleExtendedPlayerPacket(ExtendedPlayerPacket packet, IConnectedPlayer player)
		{
			if (_extendedPlayers.ContainsKey(player.userId))
			{
				ExtendedPlayer extendedPlayer = _extendedPlayers[player.userId];
				extendedPlayer.platformID = packet.platformID;
				extendedPlayer.platform = packet.platform;
				extendedPlayer.playerColor = packet.playerColor;
				extendedPlayer.mpexVersion = new Hive.Versioning.Version(packet.mpexVersion);
				extendedPlayerConnectedEvent?.Invoke(extendedPlayer);
			}
			else
			{
				Plugin.Log?.Info($"Received 'ExtendedPlayerPacket' from '{player.userId}' with platformID: '{packet.platformID}'  mpexVersion: '{packet.mpexVersion}'");
				ExtendedPlayer extendedPlayer = new ExtendedPlayer(player, packet.platformID, packet.platform, new Hive.Versioning.Version(packet.mpexVersion), packet.playerColor);

				if (Plugin.ProtocolVersion != extendedPlayer.mpexVersion)
				{
					Plugin.Log?.Warn("###################################################################");
					Plugin.Log?.Warn("Different MultiplayerExtensions protocol detected!");
					Plugin.Log?.Warn($"The player '{player.userName}' is using MpEx protocol version {extendedPlayer.mpexVersion} while you are using MpEx protocol {Plugin.ProtocolVersion}");
					Plugin.Log?.Warn("For best compatibility all players should use a compatible version of MultiplayerExtensions/MultiQuestensions.");
					Plugin.Log?.Warn("###################################################################");
				}

				_extendedPlayers[player.userId] = extendedPlayer;
				extendedPlayerConnectedEvent?.Invoke(extendedPlayer);
			}
		}

		public ExtendedPlayer? GetExtendedPlayer(IConnectedPlayer player)
			=> GetExtendedPlayer(player.userId);

		public ExtendedPlayer? GetExtendedPlayer(string userId)
		{
			if (localExtendedPlayer.userId == userId)
				return localExtendedPlayer;
			if (_extendedPlayers.TryGetValue(userId, out ExtendedPlayer extendedPlayer))
				return extendedPlayer;
			return null;
		}
	}
}
