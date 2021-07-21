using System;
using IPA.Utilities;
using MultiplayerExtensions.Extensions;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class LobbyEnvironmentManager : IInitializable, IDisposable
    {
		protected readonly ExtendedSessionManager _sessionManager;
		protected readonly ILobbyStateDataModel _lobbyStateDataModel;
		protected readonly MenuEnvironmentManager _menuEnvironmentManager;
		protected readonly MultiplayerLobbyAvatarPlaceManager _placeManager;
		protected readonly MultiplayerLobbyCenterStageManager _stageManager;

		private LobbyAvatarPlaceLighting[] _avatarPlaces = Array.Empty<LobbyAvatarPlaceLighting>();
		private float _innerCircleRadius;
		private float _minOuterCircleRadius;
		private float _angleBetweenPlayersWithEvenAdjustment;
		private float _outerCircleRadius;

		internal LobbyEnvironmentManager(IMultiplayerSessionManager sessionManager, ILobbyStateDataModel lobbyStateDataModel, MenuEnvironmentManager menuEnvironmentManager, MultiplayerLobbyAvatarPlaceManager placeManager, MultiplayerLobbyCenterStageManager stageManager)
        {
			_sessionManager = (sessionManager as ExtendedSessionManager)!;
			_lobbyStateDataModel = lobbyStateDataModel;
			_menuEnvironmentManager = menuEnvironmentManager;
			_placeManager = placeManager;
			_stageManager = stageManager;
        }

		public void Initialize()
        {
			MPEvents.LobbyEnvironmentLoaded += HandleLobbyEnvironmentLoaded;
			_sessionManager.extendedPlayerConnectedEvent += HandleExtendedPlayerConnected;
			_sessionManager.playerConnectedEvent += HandlePlayerConnected;
			_sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

		public void Dispose()
        {
			MPEvents.LobbyEnvironmentLoaded -= HandleLobbyEnvironmentLoaded;
			_sessionManager.extendedPlayerConnectedEvent -= HandleExtendedPlayerConnected;
			_sessionManager.playerConnectedEvent -= HandlePlayerConnected;
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

			SetDefaultPlayerPlaceColors();
		}

		public void SetDefaultPlayerPlaceColors()
		{
			SetAllPlayerPlaceColors(Color.black, true);
			SetPlayerPlaceColor(_sessionManager.localPlayer, ExtendedSessionManager.localExtendedPlayer.playerColor, true);
			
			foreach (var player in _sessionManager.connectedPlayers)
				SetPlayerPlaceColor(player, ExtendedPlayer.DefaultColor, false);
			
			foreach (var extendedPlayer in _sessionManager.extendedPlayers.Values)
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
			foreach (LobbyAvatarPlaceLighting place in _avatarPlaces)
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
			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, _angleBetweenPlayersWithEvenAdjustment);
			Vector3 playerWorldPosition = MultiplayerPlayerPlacement.GetPlayerWorldPosition(_outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);
			return Array.Find(_avatarPlaces, place => place.transform.position == playerWorldPosition && place.isActiveAndEnabled);
		}
	}
}
