using MultiplayerExtensions.Extensions;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class MultiplayerActivePlayer : MonoBehaviour
    {
        protected IConnectedPlayer _connectedPlayer = null!;
        protected MultiplayerController _multiplayerController = null!;
        protected ExtendedSessionManager _sessionManager = null!;
        protected IScoreSyncStateManager _scoreProvider = null!;
        protected MultiplayerLeadPlayerProvider _leadPlayerProvider = null!;

        [Inject]
        internal void Inject(IConnectedPlayer connectedPlayer, MultiplayerController multiplayerController, IMultiplayerSessionManager sessionManager, IScoreSyncStateManager scoreProvider, MultiplayerLeadPlayerProvider leadPlayerProvider)
		{
            _connectedPlayer = connectedPlayer;
            _multiplayerController = multiplayerController;
            _sessionManager = (sessionManager as ExtendedSessionManager)!;
            _scoreProvider = scoreProvider;
            _leadPlayerProvider = leadPlayerProvider;
		}

        protected void Awake()
        {
            if (Plugin.Config.MissLighting)
            {
                MultiplayerGameplayAnimator gameplayAnimator = transform.GetComponentInChildren<MultiplayerGameplayAnimator>();
                MultiplayerGameplayLighting gameplayLighting = gameplayAnimator.gameObject.AddComponent<MultiplayerGameplayLighting>();
                gameplayLighting.Construct(_connectedPlayer, _multiplayerController, _scoreProvider, _leadPlayerProvider, gameplayAnimator, _sessionManager);
            }
        }
    }
}
