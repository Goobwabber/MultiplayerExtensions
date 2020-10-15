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
        /// This code is run before the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
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
        /// This code is run before the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
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
        static void Prefix()
        {
            Plugin.Log?.Debug("Disabling CustomLevels");
            EnableCustomLevelsPatch.Enabled = false;
        }
    }
}