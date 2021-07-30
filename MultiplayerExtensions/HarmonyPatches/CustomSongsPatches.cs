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
    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), nameof(MultiplayerLobbyConnectionController.CreateParty), MethodType.Normal)]
    internal class CreatePartyPatch
    {
        /// <summary>
        /// Modifies the data used to create a party game
        /// </summary>
        static void Prefix(CreateServerFormData data)
        {
            data.songPacks = SongPackMask.all | new SongPackMask("custom_levelpack_CustomLevels");
        }
    }

    [HarmonyPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", MethodType.Getter)]
    internal class EnableCustomLevelsPatch
    {
        /// <summary>
        /// Overrides getter for <see cref="MultiplayerLevelSelectionFlowCoordinator.enableCustomLevels"/>
        /// </summary>
        static bool Prefix(ref bool __result, SongPackMask ____songPackMask)
        {
            __result = ____songPackMask.Contains(new SongPackMask("custom_levelpack_CustomLevels"));
            return false;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "DidActivate", MethodType.Normal)]
    internal class GameServerDidActivatePatch
	{
        /// <summary>
        /// Does stuff as soon as the lobby is loaded.
        /// </summary>
        static void Postfix(IUnifiedNetworkPlayerModel ____unifiedNetworkPlayerModel)
		{
            MPState.CustomSongsEnabled = ____unifiedNetworkPlayerModel.selectionMask.songPacks.Contains(new SongPackMask("custom_levelpack_CustomLevels"));
		}
	}

    [HarmonyPatch(typeof(LobbySetupViewController), nameof(LobbySetupViewController.SetPlayersMissingLevelText), MethodType.Normal)]
    internal class MissingLevelStartPatch
    {
        /// <summary>
        /// Disables starting of game if not all players have song.
        /// </summary>
        static void Prefix(LobbySetupViewController __instance, string playersMissingLevelText, ref Button ____startGameReadyButton)
        {
            if (____startGameReadyButton.interactable)
                __instance.SetStartGameEnabled(CannotStartGameReason.None);
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
            //foreach(GameplayModifierToggle toggle in ____gameplayModifierToggles)
            //{
            //    Plugin.Log.Warn(toggle.gameplayModifier.modifierNameLocalizationKey);
            //}

            bool speedModsEnabled = !MPState.FreeModEnabled || MPState.LocalPlayerIsHost;
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
