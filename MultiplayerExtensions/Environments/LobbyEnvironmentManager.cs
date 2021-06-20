using IPA.Utilities;
using MultiplayerExtensions.Sessions;
using System;
using System.Linq;
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
			_sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

		public void Dispose()
        {
			MPEvents.LobbyEnvironmentLoaded -= HandleLobbyEnvironmentLoaded;
			_playerManager.extendedPlayerConnectedEvent -= HandleExtendedPlayerConnected;
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

			SetAllPlayerPlaceColors(Color.black, true);
			SetPlayerPlaceColor(_sessionManager.localPlayer, ExtendedPlayerManager.localColor);
			foreach (ExtendedPlayer player in _playerManager.players.Values)
				SetPlayerPlaceColor(player, player.playerColor);
		}

		private void HandleExtendedPlayerConnected(ExtendedPlayer player)
			=> SetPlayerPlaceColor(player, player.playerColor);

		private void HandlePlayerDisconnected(IConnectedPlayer player)
			=> SetPlayerPlaceColor(player, Color.black);

		public void SetAllPlayerPlaceColors(Color color, bool immediate = false)
        {
			foreach (LobbyAvatarPlaceLighting place in avatarPlaces)
            {
				place.SetColor(color, immediate);
            }
		}

		public void SetPlayerPlaceColor(IConnectedPlayer player, Color color)
		{
			LobbyAvatarPlaceLighting place = GetConnectedPlayerPlace(player);
			if (place != null)
			{
				place.SetColor(color, false);
			}
		}

		public LobbyAvatarPlaceLighting GetConnectedPlayerPlace(IConnectedPlayer player)
		{
			Vector3 playerWorldPosition = GetPositionOfPlayer(player);
			return Array.Find(avatarPlaces, place => place.transform.position == playerWorldPosition && place.isActiveAndEnabled);
		}

		public Vector3 GetPositionOfPlayer(IConnectedPlayer player)
        {
			int sortIndex = _lobbyStateDataModel.localPlayer.sortIndex;
			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, angleBetweenPlayersWithEvenAdjustment);
			return MultiplayerPlayerPlacement.GetPlayerWorldPosition(outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);
		}

		public Quaternion GetRotationOfPlayer(IConnectedPlayer player)
        {
			int sortIndex = _lobbyStateDataModel.localPlayer.sortIndex;
			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, angleBetweenPlayersWithEvenAdjustment);
			return Quaternion.Euler(0f, outerCirclePositionAngleForPlayer, 0f);
		}
	}
}
