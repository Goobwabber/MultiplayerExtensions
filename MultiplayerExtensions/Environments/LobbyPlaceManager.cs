using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    class LobbyPlaceManager
    {
		[Inject]
		protected IMultiplayerSessionManager multiplayerSessionManager;

		[Inject]
		protected ILobbyStateDataModel lobbyStateDataModel;

		[Inject]
		protected MultiplayerLobbyAvatarPlaceManager multiplayerPlaceManager;

		public void SetPlayerPlaceColor(IConnectedPlayer player, Color color)
		{
			MultiplayerLobbyAvatarPlace playerPlace = GetConnectedPlayerPlace(player);
			TubeBloomPrePassLight[] lights = playerPlace.GetComponentsInChildren<TubeBloomPrePassLight>();
			foreach (TubeBloomPrePassLight light in lights)
			{
				light.color = color;
				light.Refresh();
			}
		}

		public MultiplayerLobbyAvatarPlace GetConnectedPlayerPlace(IConnectedPlayer player)
		{
			float innerCircleRadius = multiplayerPlaceManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_innerCircleRadius");
			float minOuterCircleRadius = multiplayerPlaceManager.GetField<float, MultiplayerLobbyAvatarPlaceManager>("_minOuterCircleRadius");

			float angleBetweenPlayersWithEvenAdjustment = MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment(this.lobbyStateDataModel.maxPartySize, MultiplayerPlayerLayout.Circle);
			float outerCircleRadius = Mathf.Max(MultiplayerPlayerPlacement.GetOuterCircleRadius(angleBetweenPlayersWithEvenAdjustment, innerCircleRadius), minOuterCircleRadius);
			int sortIndex = this.lobbyStateDataModel.localPlayer.sortIndex;

			float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(player.sortIndex, sortIndex, angleBetweenPlayersWithEvenAdjustment);
			Vector3 playerWorldPosition = MultiplayerPlayerPlacement.GetPlayerWorldPosition(outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);

			MultiplayerLobbyAvatarPlace[] places = Resources.FindObjectsOfTypeAll<MultiplayerLobbyAvatarPlace>();
			MultiplayerLobbyAvatarPlace playerPlace = Array.Find(places, place => place.transform.position == playerWorldPosition && place.isActiveAndEnabled);

			return playerPlace;
		}
	}
}
