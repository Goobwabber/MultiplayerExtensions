using System;
using IPA.Utilities;
using MultiplayerExtensions.Sessions;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class LobbyEnvironmentManager : IInitializable, IDisposable
    {
		protected readonly IMultiplayerSessionManager _sessionManager;
		protected readonly ILobbyStateDataModel _lobbyStateDataModel;
		protected readonly MenuEnvironmentManager _menuEnvironmentManager;
		protected readonly MultiplayerLobbyAvatarPlaceManager _placeManager;
		protected readonly MultiplayerLobbyCenterStageManager _stageManager;
		protected readonly ExtendedPlayerManager _playerManager;

		private LobbyAvatarPlaceLighting[] avatarPlaces = Array.Empty<LobbyAvatarPlaceLighting>();
		private float innerCircleRadius;
		private float minOuterCircleRadius;
		private float angleBetweenPlayersWithEvenAdjustment;
		private float outerCircleRadius;

		internal LobbyEnvironmentManager(IMultiplayerSessionManager sessionManager, ILobbyStateDataModel lobbyStateDataModel, MenuEnvironmentManager menuEnvironmentManager, MultiplayerLobbyAvatarPlaceManager placeManager, MultiplayerLobbyCenterStageManager stageManager, ExtendedPlayerManager playerManager)
        {
			_sessionManager = sessionManager;
			_lobbyStateDataModel = lobbyStateDataModel;
			_menuEnvironmentManager = menuEnvironmentManager;
			_placeManager = placeManager;
			_stageManager = stageManager;
			_playerManager = playerManager;
        }

		public void Initialize()
        {
			MPEvents.LobbyEnvironmentLoaded += HandleLobbyEnvironmentLoaded;
			_playerManager.extendedPlayerConnectedEvent += HandleExtendedPlayerConnected;
			_sessionManager.playerConnectedEvent += HandlePlayerConnected;
			_sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

		public void Dispose()
        {
			MPEvents.LobbyEnvironmentLoaded -= HandleLobbyEnvironmentLoaded;
			_playerManager.extendedPlayerConnectedEvent -= HandleExtendedPlayerConnected;
			_sessionManager.playerConnectedEvent -= HandlePlayerConnected;
			_sessionManager.playerDisconnectedEvent -= HandlePlayerDisconnected;
		}

		private void HandleLobbyEnvironmentLoaded(object sender, System.EventArgs e)
		{
			var nativeAvatarPlaces = Resources.FindObjectsOfTypeAll<MultiplayerLobbyAvatarPlace>();
			avatarPlaces = new LobbyAvatarPlaceLighting[nativeAvatarPlaces.Length];
			for (var i = 0; i < nativeAvatarPlaces.Length; i++)
			{
				var nativeAvatarPlace = nativeAvatarPlaces[i];
				
				var avatarPlace = nativeAvatarPlace.GetComponent<LobbyAvatarPlaceLighting>();
				if (avatarPlace == null)
					avatarPlace = nativeAvatarPlace.gameObject.AddComponent<LobbyAvatarPlaceLighting>();
				
				avatarPlaces[i] = avatarPlace;
			}
			
			innerCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_innerCircleRadius");
			minOuterCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_minOuterCircleRadius");
			angleBetweenPlayersWithEvenAdjustment = MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment(_lobbyStateDataModel.maxPartySize, MultiplayerPlayerLayout.Circle);
			outerCircleRadius = Mathf.Max(MultiplayerPlayerPlacement.GetOuterCircleRadius(angleBetweenPlayersWithEvenAdjustment, innerCircleRadius), minOuterCircleRadius);

			bool buildingsEnabled = _sessionManager.maxPlayerCount <= 18;
			_menuEnvironmentManager.transform.Find("Construction")?.gameObject?.SetActive(buildingsEnabled);
			_menuEnvironmentManager.transform.Find("Construction (1)")?.gameObject?.SetActive(buildingsEnabled);

			float centerScreenScale = outerCircleRadius / minOuterCircleRadius;
			_stageManager.transform.localScale = new Vector3(centerScreenScale, centerScreenScale, centerScreenScale);

			SetDefaultPlayerPlaceColors();
		}

		public void SetDefaultPlayerPlaceColors()
		{
			SetAllPlayerPlaceColors(Color.black, true);
			SetPlayerPlaceColor(_sessionManager.localPlayer, ExtendedPlayerManager.localColor, true);
			
			foreach (var player in _sessionManager.connectedPlayers)
				SetPlayerPlaceColor(player, ExtendedPlayer.DefaultColor, false);
			
			foreach (var extendedPlayer in _playerManager.players.Values)
				SetPlayerPlaceColor(extendedPlayer, extendedPlayer.playerColor, true);
		}

		private void HandleExtendedPlayerConnected(ExtendedPlayer player)
			=> SetPlayerPlaceColor(player, player.playerColor, true);

		private void HandlePlayerConnected(IConnectedPlayer player)
			=> SetPlayerPlaceColor(player, ExtendedPlayer.DefaultColor, false);
 
		private void HandlePlayerDisconnected(IConnectedPlayer player)
			=> SetPlayerPlaceColor(player, Color.black, true);

		public void SetAllPlayerPlaceColors(Color color, bool immediate = false)
        {
			foreach (LobbyAvatarPlaceLighting place in avatarPlaces)
            {
				place.SetColor(color, immediate);
            }
		}

		public void SetPlayerPlaceColor(IConnectedPlayer player, Color color, bool priority)
		{
			LobbyAvatarPlaceLighting place = GetConnectedPlayerPlace(player);
			
			if (place == null)
				return;

			if (!priority && place.TargetColor != Color.black && place.TargetColor != ExtendedPlayer.DefaultColor)
				// Priority colors are always set; non-priority colors can only override default black/blue
				return;
			
			place.SetColor(color, false);
		}

		public LobbyAvatarPlaceLighting GetConnectedPlayerPlace(IConnectedPlayer player)
		{
			int sortIndex = _lobbyStateDataModel.localPlayer.sortIndex;
			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, angleBetweenPlayersWithEvenAdjustment);
			Vector3 playerWorldPosition = MultiplayerPlayerPlacement.GetPlayerWorldPosition(outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);
			return Array.Find(avatarPlaces, place => place.transform.position == playerWorldPosition && place.isActiveAndEnabled);
		}
	}
}
