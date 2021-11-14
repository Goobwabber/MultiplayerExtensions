using BeatSaverSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Extensions
{
	public class ExtendedEntitlementChecker : NetworkPlayerEntitlementChecker
	{
		protected IMultiplayerSessionManager _sessionManager = null!;

		protected Dictionary<string, Dictionary<string, EntitlementsStatus>> _entitlementsDictionary = new Dictionary<string, Dictionary<string, EntitlementsStatus>>();
		protected Dictionary<string, Dictionary<string, TaskCompletionSource<EntitlementsStatus>>> _tcsDictionary = new Dictionary<string, Dictionary<string, TaskCompletionSource<EntitlementsStatus>>>();

		public event Action<string, string, EntitlementsStatus>? receivedEntitlementEvent;

		[Inject]
		internal void Inject(IMultiplayerSessionManager multiplayerSessionManager)
		{
			_sessionManager = multiplayerSessionManager;
		}

		public override void Start()
		{
			base.Start();

			this._rpcManager.getIsEntitledToLevelEvent -= base.HandleGetIsEntitledToLevel;
			this._rpcManager.getIsEntitledToLevelEvent += HandleGetIsEntitledToLevel;

			this._rpcManager.setIsEntitledToLevelEvent += HandleSetIsEntitledToLevel;
		}

		public override void OnDestroy()
		{
			if (this._rpcManager != null)
			{
				this._rpcManager.getIsEntitledToLevelEvent -= HandleGetIsEntitledToLevel;
				this._rpcManager.getIsEntitledToLevelEvent += base.HandleGetIsEntitledToLevel;

				this._rpcManager.setIsEntitledToLevelEvent -= HandleSetIsEntitledToLevel;
			}

			base.OnDestroy();
		}

		public override async void HandleGetIsEntitledToLevel(string userId, string levelId)
		{
			EntitlementsStatus entitlementStatus = await this.GetEntitlementStatus(levelId);
			this._rpcManager.SetIsEntitledToLevel(levelId, entitlementStatus);
		}

		public void HandleSetIsEntitledToLevel(string userId, string levelId, EntitlementsStatus entitlement)
		{
			Plugin.Log?.Info($"Entitlement from '{userId}' for '{levelId}' is {entitlement.ToString()}");

			if (!_entitlementsDictionary.ContainsKey(userId))
				_entitlementsDictionary[userId] = new Dictionary<string, EntitlementsStatus>();
			_entitlementsDictionary[userId][levelId] = entitlement;

			if (_tcsDictionary.TryGetValue(userId, out Dictionary<string, TaskCompletionSource<EntitlementsStatus>> userTcsDictionary))
				if (userTcsDictionary.TryGetValue(levelId, out TaskCompletionSource<EntitlementsStatus> entitlementTcs) && !entitlementTcs.Task.IsCompleted)
					entitlementTcs.SetResult(entitlement);

			receivedEntitlementEvent?.Invoke(userId, levelId, entitlement);
		}

		public override Task<EntitlementsStatus> GetEntitlementStatus(string levelId)
		{
			Plugin.Log?.Info($"Checking level entitlement for '{levelId}'");

			string? hash = Utilities.Utils.LevelIdToHash(levelId);
			if (hash == null)
				return base.GetEntitlementStatus(levelId);

			if (SongCore.Collections.songWithHashPresent(hash))
            {
				// The input parameter for RetrieveExtraSongData is called levelId but it only takes the level hash
				var extraSongData = SongCore.Collections.RetrieveExtraSongData(hash);
				if (extraSongData != null)
				{
					foreach (var difficultyData in extraSongData._difficulties)
					{
						if (difficultyData.additionalDifficultyData != null &&
							((difficultyData.additionalDifficultyData._requirements.Contains("Noodle Extensions") && !Plugin.IsNoodleInstalled) ||
							(difficultyData.additionalDifficultyData._requirements.Contains("Mapping Extensions") && !Plugin.IsMappingInstalled)))
							return Task.FromResult(EntitlementsStatus.NotOwned);
						else if (difficultyData.additionalDifficultyData != null)
						{
							foreach (string requirement in difficultyData.additionalDifficultyData._requirements) Plugin.Log?.Info($"Requirement found: {requirement}");
						}
						else Plugin.Log?.Debug("difficultyData.additionalDifficultyData is null");

					}
				}
				else Plugin.Log?.Debug("extraSongData is null");
				return Task.FromResult(EntitlementsStatus.Ok);
			}
			return Plugin.BeatSaver.BeatmapByHash(hash).ContinueWith<EntitlementsStatus>(r =>
			{
				Beatmap? beatmap = r.Result;
				if (beatmap == null)
					return EntitlementsStatus.NotOwned;
				else
                {
					foreach (var difficulty in beatmap.Versions[0].Difficulties)
                    {
						if ((difficulty.NoodleExtensions && !Plugin.IsNoodleInstalled) || (difficulty.MappingExtensions && !Plugin.IsMappingInstalled)) return EntitlementsStatus.NotOwned;
					}
				}
				return EntitlementsStatus.NotDownloaded;
			});
		}

		public Task<EntitlementsStatus> GetUserEntitlementStatus(string userId, string levelId)
		{
			if (Utilities.Utils.LevelIdToHash(levelId) != null && !_sessionManager.GetPlayerByUserId(userId).HasState("modded"))
				return Task.FromResult(EntitlementsStatus.NotOwned);

			if (userId == _sessionManager.localPlayer.userId)
				return GetEntitlementStatus(levelId);

			if (_entitlementsDictionary.TryGetValue(userId, out Dictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement))
					return Task.FromResult(entitlement);

			if (!_tcsDictionary.ContainsKey(userId))
				_tcsDictionary[userId] = new Dictionary<string, TaskCompletionSource<EntitlementsStatus>>();
			if (!_tcsDictionary[userId].ContainsKey(levelId))
				_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();
			_rpcManager.GetIsEntitledToLevel(levelId);
			return _tcsDictionary[userId][levelId].Task;
		}

		public EntitlementsStatus GetUserEntitlementStatusWithoutRequest(string userId, string levelId)
		{
			if (_entitlementsDictionary.TryGetValue(userId, out Dictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement))
					return entitlement;

			return EntitlementsStatus.Unknown;
		}

		public async Task WaitForOkEntitlement(string userId, string levelId, CancellationToken cancellationToken)
        {
			if (_entitlementsDictionary.TryGetValue(userId, out Dictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement) && entitlement == EntitlementsStatus.Ok)
                {
                    UI.CenterScreenLoadingPanel.Instance.playersReady++;
					Plugin.Log?.Info($"User '{userId}' for level '{levelId}' is '{entitlement}'");
					return;
				}

			if (!_tcsDictionary.ContainsKey(userId))
				_tcsDictionary[userId] = new Dictionary<string, TaskCompletionSource<EntitlementsStatus>>();
			if (!_tcsDictionary[userId].ContainsKey(levelId))
				_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();

			cancellationToken.Register(() => _tcsDictionary[userId][levelId].TrySetCanceled());

			EntitlementsStatus result = EntitlementsStatus.Unknown;
			while (result != EntitlementsStatus.Ok && !cancellationToken.IsCancellationRequested)
			{
				result = await _tcsDictionary[userId][levelId].Task.ContinueWith(t => t.IsCompleted ? t.Result : EntitlementsStatus.Unknown);
				_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();
			}
			if (!cancellationToken.IsCancellationRequested) UI.CenterScreenLoadingPanel.Instance.playersReady++;
			Plugin.Log?.Info($"User '{userId}' for level '{levelId}' is '{result}' was task cancelled {(cancellationToken.IsCancellationRequested ? "true" : "false")}");
		}
	}
}
