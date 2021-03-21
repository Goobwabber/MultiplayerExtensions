using HarmonyLib;
using System.Threading.Tasks;
using BeatSaverSharp;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", MethodType.Getter)]
    internal class EnableCustomLevelsPatch
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

    [HarmonyPatch(typeof(HostLobbySetupViewController), nameof(HostLobbySetupViewController.SetPlayersMissingLevelText), MethodType.Normal)]
    internal class MissingLevelStartPatch
    {
        /// <summary>
        /// Disables starting of game if not all players have song.
        /// </summary>
        static void Prefix(HostLobbySetupViewController __instance, string playersMissingLevelText, ref Button ____startGameButton)
        {
            if (____startGameButton.interactable)
                __instance.SetStartGameEnabled(playersMissingLevelText == null, HostLobbySetupViewController.CannotStartGameReason.None);
        }
    }

    [HarmonyPatch(typeof(NetworkPlayerEntitlementChecker), nameof(NetworkPlayerEntitlementChecker.GetEntitlementStatus), MethodType.Normal)]
    internal class CustomLevelEntitlementPatch
    {
        /// <summary>
        /// Changes the return value of the entitlement checker if it is a custom song.
        /// </summary>
        static bool Prefix(string levelId, ref Task<EntitlementsStatus> __result)
        {
            string? hash = Utilities.Utils.LevelIdToHash(levelId);
            if (hash == null)
                return true;

            if (SongCore.Collections.songWithHashPresent(hash))
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

    [HarmonyPatch(typeof(NetworkPlayerEntitlementChecker), nameof(NetworkPlayerEntitlementChecker.GetPlayerLevelEntitlementsAsync), MethodType.Normal)]
    internal class StartGameLevelEntitlementPatch
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

    [HarmonyPatch(typeof(CenterStageScreenController), nameof(CenterStageScreenController.HandleLobbyPlayersDataModelDidChange), MethodType.Normal)]
    internal class CenterStageGameDataPatch
    {
        /// <summary>
        /// Replaces selected gameplay modifiers if freemod is enabled.
        /// </summary>
        static void Postfix(string userId, ref ILobbyPlayersDataModel ____lobbyPlayersDataModel, ref ModifiersSelectionView ____modifiersSelectionView)
        {
            if (userId == ____lobbyPlayersDataModel.localUserId && MPState.FreeModEnabled)
            {
                GameplayModifiers gameplayModifiers = ____lobbyPlayersDataModel.GetPlayerGameplayModifiers(userId);
                if (gameplayModifiers != null)
                    ____modifiersSelectionView.SetGameplayModifiers(gameplayModifiers);
            }
        }
    }

    [HarmonyPatch(typeof(GameplayModifiersPanelController), nameof(GameplayModifiersPanelController.RefreshTotalMultiplierAndRankUI), MethodType.Normal)]
    internal class DisableSpeedModifiersPatch
    {
        static void Postfix(ref GameplayModifierToggle[] ____gameplayModifierToggles)
        {
            foreach(GameplayModifierToggle toggle in ____gameplayModifierToggles)
            {
                Plugin.Log.Warn(toggle.gameplayModifier.modifierNameLocalizationKey);
            }

            bool speedModsEnabled = MPState.CurrentGameType != MultiplayerGameType.Private || !MPState.FreeModEnabled || MPState.LocalPlayerIsHost;
            ____gameplayModifierToggles.ToList().Find(toggle => toggle.gameplayModifier.modifierNameLocalizationKey == "MODIFIER_SUPER_FAST_SONG").toggle.interactable = speedModsEnabled;
            ____gameplayModifierToggles.ToList().Find(toggle => toggle.gameplayModifier.modifierNameLocalizationKey == "MODIFIER_FASTER_SONG").toggle.interactable = speedModsEnabled;
            ____gameplayModifierToggles.ToList().Find(toggle => toggle.gameplayModifier.modifierNameLocalizationKey == "MODIFIER_SLOWER_SONG").toggle.interactable = speedModsEnabled;
        }
    }

    [HarmonyPatch(typeof(LobbyGameStateController), nameof(LobbyGameStateController.startedBeatmapId), MethodType.Getter)]
    internal class AprilFoolsPatch
    {
        static void Postfix(ref BeatmapIdentifierNetSerializable __result)
        {
            System.DateTime time = IPA.Utilities.Utils.CanUseDateTimeNowSafely ? System.DateTime.Now : System.DateTime.UtcNow;
            if (MPState.EasterEggsEnabled && time.Month == 4 && time.Day == 1)
            {
                if (__result != null)
                    __result = new BeatmapIdentifierNetSerializable(
                        "custom_level_103D39B43966277C5E4167AB086F404E0943891F",
                        "Standard",
                        __result.difficulty == BeatmapDifficulty.ExpertPlus ? BeatmapDifficulty.ExpertPlus : BeatmapDifficulty.Expert
                    );
            }
        }
    }
}
