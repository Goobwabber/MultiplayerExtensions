using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Sessions;
using MultiplayerExtensions.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.OverrideClasses
{
    class GameStateControllerStub : LobbyGameStateController, ILobbyHostGameStateController, ILobbyGameStateController, IDisposable
    {
        [Inject]
        protected readonly SessionManager _sessionManager;

        [Inject]
        protected readonly PacketManager _packetManager;

        public new void Activate()
        {
            _sessionManager.playerStateChangedEvent += OnPlayerStateChanged;
            base.Activate();
        }

        public new void Deactivate()
        {
            _sessionManager.playerStateChangedEvent -= OnPlayerStateChanged;
            _menuRpcManager.startedLevelEvent -= HandleRpcStartedLevel;
            _menuRpcManager.cancelledLevelStartEvent -= HandleRpcCancelledLevel;
            base.Deactivate();
        }

        public new void StartListeningToGameStart()
        {
            base.StartListeningToGameStart();
            _menuRpcManager.startedLevelEvent -= HandleRpcStartedLevel;
            _menuRpcManager.startedLevelEvent += HandleRpcStartedLevel;
            _menuRpcManager.startedLevelEvent -= base.HandleMenuRpcManagerStartedLevel;
            _menuRpcManager.cancelledLevelStartEvent -= HandleRpcCancelledLevel;
            _menuRpcManager.cancelledLevelStartEvent += HandleRpcCancelledLevel;
            _menuRpcManager.cancelledLevelStartEvent -= base.HandleMenuRpcManagerCancelledLevelStart;
        }

        public override void StopListeningToGameStart()
        {
            _menuRpcManager.startedLevelEvent -= HandleRpcStartedLevel;
            base.StopListeningToGameStart();
        }

        private void OnPlayerStateChanged(IConnectedPlayer player)
        {
            if (starting)
            {
                if (_sessionManager.connectedPlayers.All((x) => x.HasState("start_primed")) && _sessionManager.LocalPlayerHasState("start_primed"))
                {
                    Plugin.Log.Debug("All players ready, starting game.");
                    StartLevel();
                }
            }
        }

        public new void StartGame()
        {
            _sessionManager.SetLocalPlayerState("start_primed", false);
            starting = true;
            base.StartGame();
            _multiplayerLevelLoader.countdownFinishedEvent -= base.HandleMultiplayerLevelLoaderCountdownFinished;
            _multiplayerLevelLoader.countdownFinishedEvent += HandleCountdown;
        }

        public new void CancelGame()
        {
            starting = false;
            _sessionManager.SetLocalPlayerState("start_primed", false);
            _multiplayerLevelLoader.countdownFinishedEvent -= HandleCountdown;
            _multiplayerLevelLoader.countdownFinishedEvent += base.HandleMultiplayerLevelLoaderCountdownFinished;
            base.CancelGame();
        }

        public void HandleRpcStartedLevel(string userId, BeatmapIdentifierNetSerializable beatmapId, GameplayModifiers gameplayModifiers, float startTime)
        {
            _sessionManager.SetLocalPlayerState("start_primed", false);
            starting = true;
            base.HandleMenuRpcManagerStartedLevel(userId, beatmapId, gameplayModifiers, startTime);
            _multiplayerLevelLoader.countdownFinishedEvent -= base.HandleMultiplayerLevelLoaderCountdownFinished;
            _multiplayerLevelLoader.countdownFinishedEvent += HandleCountdown;
        }

        public void HandleRpcCancelledLevel(string userId)
        {
            starting = false;
            _sessionManager.SetLocalPlayerState("start_primed", false);
            _multiplayerLevelLoader.countdownFinishedEvent -= HandleCountdown;
            _multiplayerLevelLoader.countdownFinishedEvent += base.HandleMultiplayerLevelLoaderCountdownFinished;
            base.HandleMenuRpcManagerCancelledLevelStart(userId);
        }

        public void HandleCountdown(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO beatmapCharacteristic, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers)
        {
            Plugin.Log?.Debug("Map finished loading, waiting for other players...");

            this.previewBeatmapLevel = previewBeatmapLevel;
            this.beatmapDifficulty = beatmapDifficulty;
            this.beatmapCharacteristic = beatmapCharacteristic;
            this.difficultyBeatmap = difficultyBeatmap;
            this.gameplayModifiers = gameplayModifiers;

            _sessionManager.SetLocalPlayerState("start_primed", true);
            if (this._levelStartedOnTime && difficultyBeatmap != null && this._multiplayerSessionManager.localPlayer.WantsToPlayNextLevel())
            {
                OnPlayerStateChanged(_sessionManager.localPlayer);
            }
            else
            {
                StartLevel();
            }
        }

        public void StartLevel()
        {
            starting = false;
            base.HandleMultiplayerLevelLoaderCountdownFinished(previewBeatmapLevel, beatmapDifficulty, beatmapCharacteristic, difficultyBeatmap, gameplayModifiers);
        }

        private bool starting;

        private IPreviewBeatmapLevel previewBeatmapLevel;
        private BeatmapDifficulty beatmapDifficulty;
        private BeatmapCharacteristicSO beatmapCharacteristic;
        private IDifficultyBeatmap difficultyBeatmap;
        private GameplayModifiers gameplayModifiers;
    }
}
