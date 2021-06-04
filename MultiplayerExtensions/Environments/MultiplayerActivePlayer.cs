using MultiplayerExtensions.Sessions;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class MultiplayerActivePlayer : MonoBehaviour
    {
        [Inject]
        protected readonly IConnectedPlayer _connectedPlayer;

        [Inject]
        protected readonly MultiplayerController _multiplayerController;

        [Inject]
        protected readonly ExtendedPlayerManager _extendedPlayerManager;

        [Inject]
        protected readonly IScoreSyncStateManager _scoreProvider;

        [Inject]
        protected readonly MultiplayerLeadPlayerProvider _leadPlayerProvider;

        protected void Awake()
        {
            MultiplayerGameplayAnimator gameplayAnimator = transform.GetComponentInChildren<MultiplayerGameplayAnimator>();
            MultiplayerGameplayLighting gameplayLighting = gameplayAnimator.gameObject.AddComponent<MultiplayerGameplayLighting>();
            gameplayLighting.Construct(_connectedPlayer, _multiplayerController, _scoreProvider, _leadPlayerProvider, gameplayAnimator, _extendedPlayerManager);
        }
    }
}
