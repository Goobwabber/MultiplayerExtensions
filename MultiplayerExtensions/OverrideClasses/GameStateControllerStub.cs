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
        protected readonly IMultiplayerSessionManager _sessionManager;
        protected readonly PacketManager _packetManager;
        protected readonly ExtendedPlayerManager _extendedPlayerManager;

        private static readonly SemVer.Version _minVersionStartPrimed = new SemVer.Version("0.4.0");
        
        internal GameStateControllerStub(IMultiplayerSessionManager sessionManager, PacketManager packetManager, ExtendedPlayerManager extendedPlayerManager)
        {
            _sessionManager = sessionManager;
            _packetManager = packetManager;
            _extendedPlayerManager = extendedPlayerManager;
        }

        public new void Activate()
        {
            _sessionManager.playerStateChangedEvent += OnPlayerStateChanged;
            _lobbyGameState.gameStateDidChangeEvent -= base.HandleGameStateDidChange;
            _lobbyGameState.gameStateDidChangeEvent += HandleGameStateDidChange;
            base.Activate();
        }

        public new void Deactivate()
        {
            _sessionManager.playerStateChangedEvent -= OnPlayerStateChanged;
            _menuRpcManager.startedLevelEvent -= HandleRpcStartedLevel;
            _menuRpcManager.cancelledLevelStartEvent -= HandleRpcCancelledLevel;
            _lobbyGameState.gameStateDidChangeEvent -= HandleGameStateDidChange;
            _lobbyGameState.gameStateDidChangeEvent += base.HandleGameStateDidChange;
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
            _menuRpcManager.cancelledLevelStartEvent -= HandleRpcCancelledLevel;
            base.StopListeningToGameStart();
        }

        private bool IsPlayerReady(IConnectedPlayer player) 
        {
            if (player.HasState("start_primed")) return true;
            
            // player is not modded: always assume ready
            if (!player.HasState("modded")) return true;
            
            var extendedPlayer = _extendedPlayerManager.GetExtendedPlayer(player);
            // did not receive mpexVersion from player or the version is too old: assume the player is ready to prevent getting stuck at "Loading..." screen 
            if (extendedPlayer == null) return true;
            if (extendedPlayer.mpexVersion == null || extendedPlayer.mpexVersion < _minVersionStartPrimed) return true;
            
            return false;
        }
        
        private void OnPlayerStateChanged(IConnectedPlayer player)
        {
            if (starting)
            {
                if (_sessionManager.connectedPlayers.All(IsPlayerReady) && _sessionManager.LocalPlayerHasState("start_primed"))
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

        public new void HandleGameStateDidChange(MultiplayerGameState newGameState)
        {
            base.HandleGameStateDidChange(newGameState);
            MPState.CurrentGameState = newGameState;
            MPEvents.RaiseGameStateChanged(_lobbyGameState, newGameState);
        }

        public new void SetMultiplayerGameType(MultiplayerGameType multiplayerGameType)
        {
            base.SetMultiplayerGameType(multiplayerGameType);
            MPState.CurrentGameType = multiplayerGameType;
        }

        private void HandleRpcStartedLevel(string userId, BeatmapIdentifierNetSerializable beatmapId, GameplayModifiers gameplayModifiers, float startTime)
        {
            
            _sessionManager.SetLocalPlayerState("start_primed", false);
            starting = true;
            base.HandleMenuRpcManagerStartedLevel(userId, beatmapId, gameplayModifiers, startTime);
            _multiplayerLevelLoader.countdownFinishedEvent -= base.HandleMultiplayerLevelLoaderCountdownFinished;
            _multiplayerLevelLoader.countdownFinishedEvent += HandleCountdown;
        }

        private void HandleRpcCancelledLevel(string userId)
        {
            starting = false;
            _sessionManager.SetLocalPlayerState("start_primed", false);
            _multiplayerLevelLoader.countdownFinishedEvent -= HandleCountdown;
            _multiplayerLevelLoader.countdownFinishedEvent += base.HandleMultiplayerLevelLoaderCountdownFinished;
            base.HandleMenuRpcManagerCancelledLevelStart(userId);
        }

        private void HandleCountdown(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO beatmapCharacteristic, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers)
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

        private void StartLevel()
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
