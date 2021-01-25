using IPA.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace MultiplayerExtensions.Environments
{
    public class LobbyPlaceManager
    {
		protected readonly IMultiplayerSessionManager _sessionManager;
		protected readonly ILobbyStateDataModel _lobbyStateDataModel;
		protected readonly MultiplayerLobbyAvatarPlaceManager _placeManager;

		internal LobbyPlaceManager(IMultiplayerSessionManager sessionManager, ILobbyStateDataModel lobbyStateDataModel, MultiplayerLobbyAvatarPlaceManager placeManager)
        {
			_sessionManager = sessionManager;
			_lobbyStateDataModel = lobbyStateDataModel;
			_placeManager = placeManager;
        }

		public void SetAllPlayerPlaceColor(Color color)
        {
			MultiplayerLobbyAvatarPlace[] places = Resources.FindObjectsOfTypeAll<MultiplayerLobbyAvatarPlace>();
			foreach(MultiplayerLobbyAvatarPlace place in places)
            {
				TubeBloomPrePassLight[] lights = place.GetComponentsInChildren<TubeBloomPrePassLight>();
				foreach (TubeBloomPrePassLight light in lights)
				{
					light.color = color;
					light.Refresh();
				}
			}
		}

		public void SetPlayerPlaceColor(IConnectedPlayer player, Color color)
		{
			MultiplayerLobbyAvatarPlace playerPlace = GetConnectedPlayerPlace(player);
			if (playerPlace != null)
			{
				TubeBloomPrePassLight[] lights = playerPlace.GetComponentsInChildren<TubeBloomPrePassLight>();
				foreach (TubeBloomPrePassLight light in lights)
				{
					light.color = color;
					light.Refresh();
				}
			}
		}

		public void SetCenterScreenScale()
        {
			float innerCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_innerCircleRadius");
			float minOuterCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_minOuterCircleRadius");
			float angleBetweenPlayersWithEvenAdjustment = MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment(_lobbyStateDataModel.maxPartySize, MultiplayerPlayerLayout.Circle);
			float outerCircleRadius = Mathf.Max(MultiplayerPlayerPlacement.GetOuterCircleRadius(angleBetweenPlayersWithEvenAdjustment, innerCircleRadius), minOuterCircleRadius);
			float scaleRatio = outerCircleRadius / minOuterCircleRadius;
			MultiplayerLobbyCenterStageManager centerscreen = Resources.FindObjectsOfTypeAll<MultiplayerLobbyCenterStageManager>().First();
			centerscreen.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
		}

		public MultiplayerLobbyAvatarPlace GetConnectedPlayerPlace(IConnectedPlayer player)
		{
			float innerCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_innerCircleRadius");
			float minOuterCircleRadius = _placeManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_minOuterCircleRadius");

			float angleBetweenPlayersWithEvenAdjustment = MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment(_lobbyStateDataModel.maxPartySize, MultiplayerPlayerLayout.Circle);
			float outerCircleRadius = Mathf.Max(MultiplayerPlayerPlacement.GetOuterCircleRadius(angleBetweenPlayersWithEvenAdjustment, innerCircleRadius), minOuterCircleRadius);
			int sortIndex = _lobbyStateDataModel.localPlayer.sortIndex;

			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, angleBetweenPlayersWithEvenAdjustment);
			Vector3 playerWorldPosition = MultiplayerPlayerPlacement.GetPlayerWorldPosition(outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);

			MultiplayerLobbyAvatarPlace[] places = Resources.FindObjectsOfTypeAll<MultiplayerLobbyAvatarPlace>();
			MultiplayerLobbyAvatarPlace playerPlace = Array.Find(places, place => place.transform.position == playerWorldPosition && place.isActiveAndEnabled);

			return playerPlace;
		}
	}
}
