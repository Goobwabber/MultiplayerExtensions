using IPA.Utilities;
using MultiplayerExtensions.Sessions;
using System;
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

		private LobbyAvatarPlaceLighting[] _avatarPlaces = Array.Empty<LobbyAvatarPlaceLighting>();
		private float _innerCircleRadius;
		private float _minOuterCircleRadius;
		private float _angleBetweenPlayersWithEvenAdjustment;
		private float _outerCircleRadius;

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
			_avatarPlaces = new LobbyAvatarPlaceLighting[nativeAvatarPlaces.Length];
			for (var i = 0; i < nativeAvatarPlaces.Length; i++)
			{
				var nativeAvatarPlace = nativeAvatarPlaces[i];
				
				var avatarPlace = nativeAvatarPlace.GetComponent<LobbyAvatarPlaceLighting>();
				if (avatarPlace == null)
					avatarPlace = nativeAvatarPlace.gameObject.AddComponent<LobbyAvatarPlaceLighting>();
				
				_avatarPlaces[i] = avatarPlace;
			}
			
			_innerCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_innerCircleRadius");
			_minOuterCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_minOuterCircleRadius");
			_angleBetweenPlayersWithEvenAdjustment = MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment(_lobbyStateDataModel.maxPartySize, MultiplayerPlayerLayout.Circle);
			_outerCircleRadius = Mathf.Max(MultiplayerPlayerPlacement.GetOuterCircleRadius(_angleBetweenPlayersWithEvenAdjustment, _innerCircleRadius), _minOuterCircleRadius);

			bool buildingsEnabled = _sessionManager.maxPlayerCount <= 18;
			_menuEnvironmentManager.transform.Find("Construction")?.gameObject?.SetActive(buildingsEnabled);
			_menuEnvironmentManager.transform.Find("Construction (1)")?.gameObject?.SetActive(buildingsEnabled);

			float centerScreenScale = _outerCircleRadius / _minOuterCircleRadius;
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
			foreach (LobbyAvatarPlaceLighting place in _avatarPlaces)
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
			int sortIndex = _lobbyStateDataModel.localPlayer.sortIndex;
			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, _angleBetweenPlayersWithEvenAdjustment);
			Vector3 playerWorldPosition = MultiplayerPlayerPlacement.GetPlayerWorldPosition(_outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);
			return Array.Find(_avatarPlaces, place => place.transform.position == playerWorldPosition && place.isActiveAndEnabled);
		}
	}
}
