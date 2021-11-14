using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Extensions
{
    class ExtendedGameStateController : LobbyGameStateController, ILobbyGameStateController, IDisposable
    {
        protected readonly ExtendedSessionManager _sessionManager;
        protected readonly ExtendedEntitlementChecker _entitlementChecker;

        internal ExtendedGameStateController(IMultiplayerSessionManager sessionManager, NetworkPlayerEntitlementChecker entitlementChecker)
        {
            _sessionManager = (sessionManager as ExtendedSessionManager)!;
            _entitlementChecker = (entitlementChecker as ExtendedEntitlementChecker)!;
        }

        public override void Activate()
        {
            this._lobbyGameStateModel.gameStateDidChangeEvent += this.HandleGameStateChanged;
            base.Activate();

            (this as ILobbyGameStateController).lobbyStateChangedEvent += this.HandleLobbyStateChanged;
        }

		public override void Deactivate()
		{
            this._lobbyGameStateModel.gameStateDidChangeEvent -= this.HandleGameStateChanged;

            this._menuRpcManager.startedLevelEvent -= this.HandleMenuRpcManagerStartedLevel;
            this._menuRpcManager.startedLevelEvent += base.HandleMenuRpcManagerStartedLevel;
            base.Deactivate();

            (this as ILobbyGameStateController).lobbyStateChangedEvent -= this.HandleLobbyStateChanged;
        }

        public override void Dispose()
        {
            this.Deactivate();
        }



		private void HandleGameStateChanged(MultiplayerGameState newGameState)
        {
            MPState.CurrentGameState = newGameState;
        }

        private void HandleLobbyStateChanged(MultiplayerLobbyState newLobbyState)
		{
            MPState.CurrentLobbyState = newLobbyState;
        }



        public override void StartListeningToGameStart()
        {
            base.StartListeningToGameStart();
            this._menuRpcManager.startedLevelEvent -= base.HandleMenuRpcManagerStartedLevel;
            this._menuRpcManager.startedLevelEvent += this.HandleMenuRpcManagerStartedLevel;
        }

        public override void StopListeningToGameStart()
        {
            this._menuRpcManager.startedLevelEvent -= this.HandleMenuRpcManagerStartedLevel;
            this._menuRpcManager.startedLevelEvent += base.HandleMenuRpcManagerStartedLevel;
            base.StopListeningToGameStart();
        }

        public override void HandleMenuRpcManagerStartedLevel(string userId, BeatmapIdentifierNetSerializable beatmapId, GameplayModifiers gameplayModifiers, float startTime)
		{
			base.HandleMenuRpcManagerStartedLevel(userId, beatmapId, gameplayModifiers, startTime);
            _multiplayerLevelLoader.countdownFinishedEvent -= base.HandleMultiplayerLevelLoaderCountdownFinished;
            _multiplayerLevelLoader.countdownFinishedEvent += this.HandleMultiplayerLevelLoaderCountdownFinished;
        }

		public override void StopLoading()
		{
            _multiplayerLevelLoader.countdownFinishedEvent += base.HandleMultiplayerLevelLoaderCountdownFinished;
            _multiplayerLevelLoader.countdownFinishedEvent -= this.HandleMultiplayerLevelLoaderCountdownFinished;

            if (_levelLoadSyncCts != null)
                _levelLoadSyncCts.Cancel();

            base.StopLoading();
		}

        private CancellationTokenSource? _levelLoadSyncCts;

        public async override void HandleMultiplayerLevelLoaderCountdownFinished(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO beatmapCharacteristic, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers)
		{
            if (_multiplayerLevelLoader.GetField<MultiplayerLevelLoader.MultiplayerBeatmapLoaderState, MultiplayerLevelLoader>("_loaderState") == MultiplayerLevelLoader.MultiplayerBeatmapLoaderState.NotLoading)
            {
                return;
            }
            _multiplayerLevelLoader.SetField("_loaderState", MultiplayerLevelLoader.MultiplayerBeatmapLoaderState.NotLoading);
            Plugin.Log?.Debug("Map finished loading, waiting for other players...");
            UI.CenterScreenLoadingPanel.Instance.playersReady = 0;
            _menuRpcManager.SetIsEntitledToLevel(previewBeatmapLevel.levelID, EntitlementsStatus.Ok);

            if (_levelStartedOnTime == false)
            {
                Plugin.Log?.Debug("Loaded level late, starting game.");
                base.HandleMultiplayerLevelLoaderCountdownFinished(previewBeatmapLevel, beatmapDifficulty, beatmapCharacteristic, difficultyBeatmap, gameplayModifiers);
                return;
            }

            _levelLoadSyncCts = new CancellationTokenSource();
            IEnumerable<Task> playerReadyTasks = _sessionManager.connectedPlayers.Select(p => p.isMe 
                ? Task.CompletedTask 
                : _entitlementChecker.WaitForOkEntitlement(p.userId, previewBeatmapLevel.levelID, _levelLoadSyncCts.Token)
            );
            await Task.WhenAll(playerReadyTasks);

            if (_levelLoadSyncCts.IsCancellationRequested) {
                _levelLoadSyncCts = null;
                return;
            }
            _levelLoadSyncCts = null;

            Plugin.Log?.Debug("All players ready, starting game.");


            base.HandleMultiplayerLevelLoaderCountdownFinished(previewBeatmapLevel, beatmapDifficulty, beatmapCharacteristic, difficultyBeatmap, gameplayModifiers);
        }
    }
}
