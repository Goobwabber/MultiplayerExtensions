using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using HMUI;
using IPA.Logging;
using IPA.Utilities;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace MultiplayerExtensions.HarmonyPatches
{
#if DEBUG
    [HarmonyPatch(typeof(SongPackMasksModel), MethodType.Constructor,
        new Type[] { // List the Types of the method's parameters.
        typeof(BeatmapLevelsModel) })]
    public class SongPackMasksModel_Constructor
    {
        /// <summary>
        /// Adds a level pack selection to Quick Play's picker. Unfortunately, the server doesn't allow custom songs to be played in Quick Play.
        /// Left here for testing.
        /// </summary>
        static void Postfix(SongPackMasksModel __instance, ref BeatmapLevelsModel beatmapLevelsModel, ref List<Tuple<SongPackMask, string>> ____songPackMaskData)
        {
            SongPackMask customs = new SongPackMask("custom_levelpack_CustomLevels");
            ____songPackMaskData.Add(customs, "Custom");
        }
    }
#endif

    [HarmonyPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", MethodType.Getter)]
    public class EnableCustomLevelsPatch
    {
        public static bool Enabled;
        /// <summary>
        /// Overrides getter for <see cref="MultiplayerLevelSelectionFlowCoordinator.enableCustomLevels"/>
        /// </summary>
        static bool Prefix(ref bool __result)
        {
            Plugin.Log?.Debug($"CustomLevels are {(Enabled ? "enabled" : "disabled")}.");
            __result = Enabled;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), "DidActivate",
        new Type[] { // List the Types of the method's parameters.
        typeof(bool), typeof(bool), typeof(bool) })]
    public class GameServerLobbyFlowCoordinator_DidActivate
    {
        /// <summary>
        /// Enables custom levels if GameServerLobbyFlowCoordinator.DidActivate is called.
        /// </summary>
        static void Prefix()
        {
            Plugin.Log?.Debug("Enabling CustomLevels");
            EnableCustomLevelsPatch.Enabled = true;
        }
    }

    [HarmonyPatch(typeof(QuickPlayLobbyFlowCoordinator), "DidActivate",
        new Type[] { // List the Types of the method's parameters.
        typeof(bool), typeof(bool), typeof(bool) })]
    public class QuickPlayLobbyFlowCoordinator_DidActivate
    {
        /// <summary>
        /// Disables custom levels if QuickPlayLobbyFlowCoordinator.DidActivate is called.
        /// </summary>
        static void Prefix()
        {
            Plugin.Log?.Debug("Disabling CustomLevels");
            EnableCustomLevelsPatch.Enabled = false;
        }
    }
}