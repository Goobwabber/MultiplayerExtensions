using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Extensions
{
    class ExtendedGameStateController : LobbyGameStateController, ILobbyGameStateController, IDisposable
    {
        protected readonly ExtendedSessionManager _sessionManager;
        protected readonly ExtendedEntitlementChecker _entitlementChecker;

        private static readonly SemVer.Version _minVersionStart = new SemVer.Version("0.6.0");

        internal ExtendedGameStateController(IMultiplayerSessionManager sessionManager, NetworkPlayerEntitlementChecker entitlementChecker)
        {
            _sessionManager = (sessionManager as ExtendedSessionManager)!;
            _entitlementChecker = (entitlementChecker as ExtendedEntitlementChecker)!;
        }

        public override void Activate()
        {
            this._lobbyGameStateModel.gameStateDidChangeEvent += this.HandleGameStateChanged;
            this._entitlementChecker.receivedEntitlementEvent += this.HandleEntitlement;
            base.Activate();

            (this as ILobbyGameStateController).lobbyStateChangedEvent += this.HandleLobbyStateChanged;
        }

		public override void Deactivate()
		{
            this._lobbyGameStateModel.gameStateDidChangeEvent -= this.HandleGameStateChanged;
            this._entitlementChecker.receivedEntitlementEvent -= this.HandleEntitlement;

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



        private IPreviewBeatmapLevel? _previewBeatmapLevel;
        private BeatmapDifficulty _beatmapDifficulty;
        private BeatmapCharacteristicSO? _beatmapCharacteristic;
        private IDifficultyBeatmap? _difficultyBeatmap;
        private GameplayModifiers? _gameplayModifiers;

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
            base.StopLoading();
		}

        public override void HandleMultiplayerLevelLoaderCountdownFinished(IPreviewBeatmapLevel previewBeatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO beatmapCharacteristic, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers)
		{
            Plugin.Log?.Debug("Map finished loading, waiting for other players...");

            this._previewBeatmapLevel = previewBeatmapLevel;
            this._beatmapDifficulty = beatmapDifficulty;
            this._beatmapCharacteristic = beatmapCharacteristic;
            this._difficultyBeatmap = difficultyBeatmap;
            this._gameplayModifiers = gameplayModifiers;

            _menuRpcManager.SetIsEntitledToLevel(previewBeatmapLevel.levelID, EntitlementsStatus.Ok);
            HandleEntitlement(_lobbyPlayersDataModel.localUserId, startedBeatmapId.levelID, EntitlementsStatus.Ok);
        }



        private async Task<bool> IsPlayerReady(IConnectedPlayer player)
        {
            if (!player.HasState("modded")) return true; // player is not modded: always assume ready
            if (await _entitlementChecker.GetUserEntitlementStatusWithoutRequest(player.userId, startedBeatmapId.levelID) == EntitlementsStatus.Ok) return true;

            ExtendedPlayer? extendedPlayer = _sessionManager.GetExtendedPlayer(player);

            // did not recieve mpexVersion from player or the version is too old: assume the player is ready
            if (extendedPlayer == null) return true;
            if (extendedPlayer.mpexVersion == null || extendedPlayer.mpexVersion < _minVersionStart) return true;

            return false;
		}

        private async void HandleEntitlement(string userId, string levelId, EntitlementsStatus entitlement)
        {
            if (state == MultiplayerLobbyState.GameStarting)
			{
                if (_levelStartedOnTime == false)
				{
                    Plugin.Log.Debug("Loaded level late, starting game.");
                    base.HandleMultiplayerLevelLoaderCountdownFinished(_previewBeatmapLevel, _beatmapDifficulty, _beatmapCharacteristic, _difficultyBeatmap, _gameplayModifiers);
                }

                IEnumerable<Task<bool>> readyTasks = _sessionManager.connectedPlayers.Select(IsPlayerReady);
                bool[] readyStates = await Task.WhenAll<bool>(readyTasks);
                if (readyStates.All(x => x) && await _entitlementChecker.GetEntitlementStatus(startedBeatmapId.levelID) == EntitlementsStatus.Ok)
				{
                    Plugin.Log.Debug("All players ready, starting game.");
                    base.HandleMultiplayerLevelLoaderCountdownFinished(_previewBeatmapLevel, _beatmapDifficulty, _beatmapCharacteristic, _difficultyBeatmap, _gameplayModifiers);
                }
			}
        }
    }
}
