using IPA.Utilities;
using MultiplayerExtensions.Extensions;
using UnityEngine;

namespace MultiplayerExtensions.Environments
{
    class MultiplayerGameplayLighting : MonoBehaviour
    {
        protected IConnectedPlayer _connectedPlayer = null!;
        protected MultiplayerController _multiplayerController = null!;
        protected IScoreSyncStateManager _scoreProvider = null!;
        protected MultiplayerLeadPlayerProvider _leadPlayerProvider = null!;
        protected MultiplayerGameplayAnimator _gameplayAnimator = null!;
        protected ExtendedSessionManager _sessionManager = null!;

        protected LightsAnimator[] _allLights = null!;
        protected LightsAnimator[] _gameplayLights = null!;

        protected ColorSO _activeLightsColor = null!;
        protected ColorSO _leadingLightsColor = null!;
        protected ColorSO _failedLightsColor = null!;
        protected Color _missLightsColor = new Color(1, 0, 0);

        protected bool _isLeading = false;
        protected int _highestCombo = 0;

        protected MultiplayerSyncState<StandardScoreSyncState, StandardScoreSyncState.Score, int> _syncState = null!;

        internal void Construct(IConnectedPlayer connectedPlayer, MultiplayerController multiplayerController, IScoreSyncStateManager scoreProvider, MultiplayerLeadPlayerProvider leadPlayerProvider, MultiplayerGameplayAnimator gameplayAnimator, ExtendedSessionManager sessionManager)
        {
            _connectedPlayer = connectedPlayer;
            _multiplayerController = multiplayerController;
            _scoreProvider = scoreProvider;
            _leadPlayerProvider = leadPlayerProvider;
            _gameplayAnimator = gameplayAnimator;
            _sessionManager = sessionManager;

            _allLights = gameplayAnimator.GetField<LightsAnimator[], MultiplayerGameplayAnimator>("_allLightsAnimators");
            _gameplayLights = gameplayAnimator.GetField<LightsAnimator[], MultiplayerGameplayAnimator>("_gameplayLightsAnimators");

            _activeLightsColor = gameplayAnimator.GetField<ColorSO, MultiplayerGameplayAnimator>("_activeLightsColor");
            _leadingLightsColor = gameplayAnimator.GetField<ColorSO, MultiplayerGameplayAnimator>("_leadingLightsColor");
            _failedLightsColor = gameplayAnimator.GetField<ColorSO, MultiplayerGameplayAnimator>("_failedLightsColor");

            _leadPlayerProvider.newLeaderWasSelectedEvent += this.HandleNewLeaderWasSelected;
        }

        protected void Update()
        {
            if (_multiplayerController.state == MultiplayerController.State.Gameplay)
            {
                if (!this._connectedPlayer.IsFailed())
                {
                    if (_syncState == null)
                        _syncState = _scoreProvider.GetSyncStateForPlayer(_connectedPlayer);

                    int combo = _syncState.GetState(StandardScoreSyncState.Score.Combo, _syncState.player.offsetSyncTime);
                    if (combo > _highestCombo)
                        _highestCombo = combo;

                    Color baseColor = _isLeading ? _leadingLightsColor : _activeLightsColor;
                    _missLightsColor.a = baseColor.a;
                    float failPercentage = (Mathf.Min(_highestCombo, 20f) - combo) / 20f;
                    
                    SetLights(Color.Lerp(baseColor, _missLightsColor, failPercentage));
                }
            }
        }

        protected void HandleNewLeaderWasSelected(string userId)
        {
            if (this._connectedPlayer.IsFailed())
                return;
            _isLeading = userId == this._connectedPlayer.userId;
        }

        public void SetLights(Color color)
        {
            foreach (LightsAnimator light in _gameplayLights)
            {
                light.SetColor(color);
            }
        }
    }
}
