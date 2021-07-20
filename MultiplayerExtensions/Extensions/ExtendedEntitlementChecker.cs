using BeatSaverSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.Extensions
{
	public class ExtendedEntitlementChecker : NetworkPlayerEntitlementChecker
	{
		protected IMultiplayerSessionManager _sessionManager = null!;

		protected Dictionary<string, Dictionary<string, EntitlementsStatus>> _entitlementsDictionary = new Dictionary<string, Dictionary<string, EntitlementsStatus>>();
		protected Dictionary<string, Dictionary<string, TaskCompletionSource<EntitlementsStatus>>> _tcsDictionary = new Dictionary<string, Dictionary<string, TaskCompletionSource<EntitlementsStatus>>>();

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

		public override void HandleGetIsEntitledToLevel(string userId, string levelId)
		{
			base.HandleGetIsEntitledToLevel(userId, levelId);
		}

		public void HandleSetIsEntitledToLevel(string userId, string levelId, EntitlementsStatus entitlement)
		{
			_entitlementsDictionary[userId][levelId] = entitlement;

			if (_tcsDictionary.TryGetValue(userId, out Dictionary<string, TaskCompletionSource<EntitlementsStatus>> userDictionary))
				if (userDictionary.TryGetValue(levelId, out TaskCompletionSource<EntitlementsStatus> entitlementTcs))
					entitlementTcs.SetResult(entitlement);
		}

		public override Task<EntitlementsStatus> GetEntitlementStatus(string levelId)
		{
			Plugin.Log?.Debug($"Checking level entitlement for '{levelId}'");

			string? hash = Utilities.Utils.LevelIdToHash(levelId);
			if (hash == null)
				return base.GetEntitlementStatus(levelId);

			if (SongCore.Collections.songWithHashPresent(hash))
				return Task.FromResult(EntitlementsStatus.Ok);
			return Plugin.BeatSaver.Hash(hash).ContinueWith<EntitlementsStatus>(r =>
			{
				Beatmap? beatmap = r.Result;
				if (beatmap == null)
					return EntitlementsStatus.NotOwned;
				return EntitlementsStatus.NotDownloaded;
			});
		}

		public Task<EntitlementsStatus> GetUserEntitlementStatus(string userId, string levelId)
		{
			if (userId == _sessionManager.localPlayer.userId)
				return GetEntitlementStatus(levelId);

			if (_entitlementsDictionary.TryGetValue(userId, out Dictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement))
					return Task.FromResult(entitlement);

			_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();
			_rpcManager.GetIsEntitledToLevel(levelId);
			return _tcsDictionary[userId][levelId].Task;
		}
	}
}
