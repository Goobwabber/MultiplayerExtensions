using IPA.Utilities;
using System;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environment
{
    class MpexPlayerFacadeLighting : MonoBehaviour
    {
        private readonly FieldAccessor<MultiplayerGameplayAnimator, LightsAnimator[]>.Accessor _allLightsAnimators
            = FieldAccessor<MultiplayerGameplayAnimator, LightsAnimator[]>
                .GetAccessor(nameof(_allLightsAnimators));
        private readonly FieldAccessor<MultiplayerGameplayAnimator, LightsAnimator[]>.Accessor _gameplayLightsAnimators
            = FieldAccessor<MultiplayerGameplayAnimator, LightsAnimator[]>
                .GetAccessor(nameof(_gameplayLightsAnimators));

        private readonly FieldAccessor<MultiplayerGameplayAnimator, ColorSO>.Accessor _activeLightsColor
            = FieldAccessor<MultiplayerGameplayAnimator, ColorSO>
                .GetAccessor(nameof(_activeLightsColor));
        private readonly FieldAccessor<MultiplayerGameplayAnimator, ColorSO>.Accessor _leadingLightsColor
            = FieldAccessor<MultiplayerGameplayAnimator, ColorSO>
                .GetAccessor(nameof(_leadingLightsColor));
        private readonly FieldAccessor<MultiplayerGameplayAnimator, ColorSO>.Accessor _failedLightsColor
            = FieldAccessor<MultiplayerGameplayAnimator, ColorSO>
                .GetAccessor(nameof(_failedLightsColor));

        private LightsAnimator[] _allLights => _allLightsAnimators(ref _gameplayAnimator);
        private LightsAnimator[] _gameplayLights => _gameplayLightsAnimators(ref _gameplayAnimator);

        private ColorSO _activeColor => _activeLightsColor(ref _gameplayAnimator);
        private ColorSO _leadingColor => _leadingLightsColor(ref _gameplayAnimator);
        private ColorSO _failedColor => _failedLightsColor(ref _gameplayAnimator);

        private bool _isLeading = false;
        private int _highestCombo = 0;

        private IConnectedPlayer _connectedPlayer = null!;
        private MultiplayerController _multiplayerController = null!;
        private IScoreSyncStateManager _scoreProvider = null!;
        private MultiplayerLeadPlayerProvider _leadPlayerProvider = null!;
        private MultiplayerGameplayAnimator _gameplayAnimator = null!;
        private MultiplayerSyncState<StandardScoreSyncState, StandardScoreSyncState.Score, int> _syncState = null!;
        private Config _config = null!;

        [Inject]
        internal void Construct(
            IConnectedPlayer connectedPlayer, 
            MultiplayerController multiplayerController, 
            IScoreSyncStateManager scoreProvider, 
            MultiplayerLeadPlayerProvider leadPlayerProvider,
            Config config)
        {
            _connectedPlayer = connectedPlayer;
            _multiplayerController = multiplayerController;
            _scoreProvider = scoreProvider;
            _leadPlayerProvider = leadPlayerProvider;
            _config = config;
        }

        public void OnEnable()
        {
            _gameplayAnimator = GetComponent<MultiplayerGameplayAnimator>();
            _syncState = _scoreProvider.GetSyncStateForPlayer(_connectedPlayer);
            _leadPlayerProvider.newLeaderWasSelectedEvent += HandleNewLeaderWasSelected;
        }

        public void OnDisable()
        {
            _leadPlayerProvider.newLeaderWasSelectedEvent -= HandleNewLeaderWasSelected;
        }

        private void HandleNewLeaderWasSelected(string userId)
        {
            _isLeading = userId == _connectedPlayer.userId;
        }

        private void Update()
        {
            if (_multiplayerController.state == MultiplayerController.State.Gameplay
                && !_connectedPlayer.IsFailed())
            {
                int combo = _syncState.GetState(StandardScoreSyncState.Score.Combo, _syncState.player.offsetSyncTime);
                if (combo > _highestCombo)
                    _highestCombo = combo;

                Color baseColor = _isLeading ? _leadingColor : _activeColor;
                float failPercentage = (Mathf.Min(_highestCombo, 20f) - combo) / 20f;
                Color color = _config.MissColor;
                color.a = baseColor.a;
                SetLights(Color.Lerp(baseColor, color, failPercentage));
            }
        }

        public void SetLights(Color color)
        {
            foreach (LightsAnimator light in _gameplayLightsAnimators(ref _gameplayAnimator))
                light.SetColor(color);
        }
    }
}
