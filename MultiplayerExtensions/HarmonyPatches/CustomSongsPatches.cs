using HarmonyLib;
using System.Threading.Tasks;
using BeatSaverSharp;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", MethodType.Getter)]
    public class EnableCustomLevelsPatch
    {
        /// <summary>
        /// Overrides getter for <see cref="MultiplayerLevelSelectionFlowCoordinator.enableCustomLevels"/>
        /// </summary>
        static bool Prefix(ref bool __result)
        {
            __result = MPState.CurrentGameType == MultiplayerGameType.Private && MPState.CustomSongsEnabled;
            return false;
        }
    }

    [HarmonyPatch(typeof(NetworkPlayerEntitlementChecker), "GetEntitlementStatus", MethodType.Normal)]
    public class CustomLevelEntitlementPatch
    {
        /// <summary>
        /// Changes the return value of the entitlement checker if it is a custom song.
        /// </summary>
        static bool Prefix(string levelId, ref Task<EntitlementsStatus> __result)
        {
            string? hash = Utilities.Utils.LevelIdToHash(levelId);
            if (hash == null)
                return true;

            if (SongCore.Loader.GetLevelByHash(hash) != null)
                __result = Task.FromResult(EntitlementsStatus.Ok);
            else
                __result = Plugin.BeatSaver.Hash(hash).ContinueWith<EntitlementsStatus>(r =>
                {
                    Beatmap beatmap = r.Result;
                    if (beatmap == null)
                        return EntitlementsStatus.NotOwned;
                    return EntitlementsStatus.NotDownloaded;
                });

            return false;
        }
    }

    [HarmonyPatch(typeof(NetworkPlayerEntitlementChecker), "GetPlayerLevelEntitlementsAsync", MethodType.Normal)]
    public class StartGameLevelEntitlementPatch
    {
        /// <summary>
        /// Changes the return value if it returns 'NotDownloaded' so that the host can start the game.
        /// </summary>
        static void Postfix(ref Task<EntitlementsStatus> __result)
        {
            __result = __result.ContinueWith(r =>
            {
                if (r.Result == EntitlementsStatus.NotDownloaded)
                    return EntitlementsStatus.Ok;
                else
                    return r.Result;
            });
        }
    }
}
