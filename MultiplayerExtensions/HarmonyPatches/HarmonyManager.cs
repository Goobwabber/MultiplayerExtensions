using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MultiplayerExtensions.HarmonyPatches
{
    public static class HarmonyManager
    {
        public static readonly string HarmonyId = "com.github.Zingabopp.MultiplayerExtensions";
        private static Harmony? _harmony;
        internal static Harmony Harmony
        {
            get
            {
                return _harmony ??= new Harmony(HarmonyId);
            }
        }
        internal static readonly BindingFlags allBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static HarmonyPatchInfo? EnableCustomLevelsPatch;
        private static HarmonyPatchInfo? LobbyJoinPatch;
        private static HarmonyPatchInfo? LevelCollectionViewController_DidSelectLevel;
        private static HarmonyPatchInfo? MultiplayerBigAvatarAnimator_Init;
        private static HarmonyPatchInfo? CoreGameHUDController_Start;
        private static HarmonyPatchInfo? MultiplayerSessionManager_HandlePlayerConnected;
        private static HarmonyPatchInfo? MultiplayerSessionManager_PlayerStateChanged;
        private static HarmonyPatchInfo? MultiplayerSessionManager_HandleConnected;
        private static HarmonyPatchInfo? LobbyGameStateController_HandleMenuRpcManagerStartedLevel;
        private static HarmonyPatchInfo? MultiplayerLevelLoader_LoadLevel;
        private static HarmonyPatchInfo? SetPlayerLevelPatch;
        private static HarmonyPatchInfo? SetLocalPlayerLevelPatch;

        internal static readonly HashSet<HarmonyPatchInfo> AppliedPatches = new HashSet<HarmonyPatchInfo>();
        internal static readonly HashSet<HarmonyPatchInfo> DefaultPatches = new HashSet<HarmonyPatchInfo>();

        static HarmonyManager()
        {
            DefaultPatches.Add(GetEnableCustomLevelsPatch());
            DefaultPatches.Add(GetLobbyJoinPatch());
            DefaultPatches.Add(GetLevelCollectionViewController_DidSelectLevel());
            DefaultPatches.Add(GetMultiplayerBigAvatarAnimator_Init());
            DefaultPatches.Add(GetCoreGameHUDController_Start());
            // TODO: Wasn't being applied before?
            // DefaultPatches.Add(GetMultiplayerSessionManager_HandlePlayerConnected());
            DefaultPatches.Add(GetMultiplayerSessionManager_PlayerStateChanged());
            DefaultPatches.Add(GetMultiplayerSessionManager_HandleConnected());
            DefaultPatches.Add(GetLobbyGameStateController_HandleMenuRpcManagerStartedLevel());
            DefaultPatches.Add(GetMultiplayerLevelLoader_LoadLevel());
            DefaultPatches.Add(GetSetPlayerLevelPatch());
            DefaultPatches.Add(GetSetLocalPlayerLevelPatch());
        }

        public static bool ApplyPatch(HarmonyPatchInfo patchInfo)
        {
            bool applied = patchInfo.ApplyPatch(Harmony);
            if (applied)
                AppliedPatches.Add(patchInfo);
            return applied;
        }

        public static bool RemovePatch(HarmonyPatchInfo patchInfo)
        {
            bool removed = patchInfo.RemovePatch(Harmony);
            if (removed)
                AppliedPatches.Remove(patchInfo);
            return removed;
        }

        public static bool ApplyPatch(Harmony harmony, MethodInfo original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null)
        {
            try
            {
                string? patchTypeName = null;
                if (prefix != null)
                    patchTypeName = prefix.method.DeclaringType?.Name;
                else if (postfix != null)
                    patchTypeName = postfix.method.DeclaringType?.Name;
                Plugin.Log?.Debug($"Harmony patching {original.Name} with {patchTypeName}");
                harmony.Patch(original, prefix, postfix);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log?.Error($"Unable to patch method {original.Name}: {e.Message}");
                Plugin.Log?.Debug(e);
                return false;
            }
        }

        public static void ApplyDefaultPatches()
        {
            HarmonyPatchInfo[] patches = DefaultPatches.ToArray();
            Plugin.Log?.Debug($"Applying {patches.Length} Harmony patches.");
            for (int i = 0; i < patches.Length; i++)
                ApplyPatch(patches[i]);
        }

        public static void UnpatchAll()
        {
            foreach (HarmonyPatchInfo? patch in AppliedPatches.ToList())
            {
                patch.RemovePatch();
            }
            Harmony.UnpatchAll(HarmonyId);
        }
        #region EnableCustomSongsPatches
        private static HarmonyPatchInfo GetEnableCustomLevelsPatch()
        {
            if (EnableCustomLevelsPatch == null)
            {
                MethodInfo original = typeof(MultiplayerLevelSelectionFlowCoordinator).GetProperty("enableCustomLevels", allBindingFlags).GetMethod;
                HarmonyMethod prefix = new HarmonyMethod(typeof(EnableCustomLevelsPatch).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                EnableCustomLevelsPatch = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return EnableCustomLevelsPatch;
        }
        private static HarmonyPatchInfo GetLobbyJoinPatch()
        {
            if (LobbyJoinPatch == null)
            {
                MethodInfo original = typeof(MultiplayerLobbyConnectionController).GetProperty("connectionType", allBindingFlags).SetMethod;
                HarmonyMethod? prefix = new HarmonyMethod(typeof(LobbyJoinPatch).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                LobbyJoinPatch = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return LobbyJoinPatch;
        }
        #endregion
        #region InterfacePatches
        private static HarmonyPatchInfo GetLevelCollectionViewController_DidSelectLevel()
        {
            if (LevelCollectionViewController_DidSelectLevel == null)
            {
                MethodInfo original = typeof(LevelCollectionViewController).GetMethod("HandleLevelCollectionTableViewDidSelectLevel", allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(LevelCollectionViewController_DidSelectLevel).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                LevelCollectionViewController_DidSelectLevel = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return LevelCollectionViewController_DidSelectLevel;
        }

        private static HarmonyPatchInfo GetMultiplayerBigAvatarAnimator_Init()
        {
            if (MultiplayerBigAvatarAnimator_Init == null)
            {
                MethodInfo original = typeof(MultiplayerBigAvatarAnimator).GetMethod("InitIfNeeded", allBindingFlags);
                HarmonyMethod? prefix = null;
                HarmonyMethod postfix = new HarmonyMethod(typeof(MultiplayerBigAvatarAnimator_Init).GetMethod("Postfix", allBindingFlags));
                MultiplayerBigAvatarAnimator_Init = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return MultiplayerBigAvatarAnimator_Init;
        }

        private static HarmonyPatchInfo GetCoreGameHUDController_Start()
        {
            if (CoreGameHUDController_Start == null)
            {
                MethodInfo original = typeof(CoreGameHUDController).GetMethod("Start", allBindingFlags);
                HarmonyMethod? prefix = null;
                HarmonyMethod? postfix = new HarmonyMethod(typeof(CoreGameHUDController_Start).GetMethod("Postfix", allBindingFlags));
                CoreGameHUDController_Start = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return CoreGameHUDController_Start;
        }

        #endregion

        private static HarmonyPatchInfo GetMultiplayerSessionManager_HandlePlayerConnected()
        {
            if (MultiplayerSessionManager_HandlePlayerConnected == null)
            {
                MethodInfo original = typeof(MultiplayerSessionManager).GetMethod("HandlePlayerConnected", allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(MultiplayerSessionManager_HandlePlayerConnected).GetMethod("Postfix", allBindingFlags));
                HarmonyMethod? postfix = null;
                MultiplayerSessionManager_HandlePlayerConnected = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return MultiplayerSessionManager_HandlePlayerConnected;
        }

        private static HarmonyPatchInfo GetMultiplayerSessionManager_PlayerStateChanged()
        {
            if (MultiplayerSessionManager_PlayerStateChanged == null)
            {
                MethodInfo original = typeof(MultiplayerSessionManager).GetMethod("HandlePlayerStateChanged", allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(MultiplayerSessionManager_PlayerStateChanged).GetMethod("Postfix", allBindingFlags));
                HarmonyMethod? postfix = null;
                MultiplayerSessionManager_PlayerStateChanged = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return MultiplayerSessionManager_PlayerStateChanged;
        }

        private static HarmonyPatchInfo GetMultiplayerSessionManager_HandleConnected()
        {
            if (MultiplayerSessionManager_HandleConnected == null)
            {
                MethodInfo original = typeof(MultiplayerSessionManager).GetMethod("Start", allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(MultiplayerSessionManager_HandleConnected).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                MultiplayerSessionManager_HandleConnected = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return MultiplayerSessionManager_HandleConnected;
        }

        private static HarmonyPatchInfo GetLobbyGameStateController_HandleMenuRpcManagerStartedLevel()
        {
            if (LobbyGameStateController_HandleMenuRpcManagerStartedLevel == null)
            {
                MethodInfo original = typeof(LobbyGameStateController).GetMethod(nameof(LobbyGameStateController.HandleMenuRpcManagerStartedLevel), allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(LobbyGameStateController_HandleMenuRpcManagerStartedLevel).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                LobbyGameStateController_HandleMenuRpcManagerStartedLevel = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return LobbyGameStateController_HandleMenuRpcManagerStartedLevel;
        }


        private static HarmonyPatchInfo GetMultiplayerLevelLoader_LoadLevel()
        {
            if (MultiplayerLevelLoader_LoadLevel == null)
            {
                Type[] parameters = new Type[] { typeof(BeatmapIdentifierNetSerializable), typeof(GameplayModifiers), typeof(float) };
                MethodInfo original = typeof(MultiplayerLevelLoader)
                    .GetMethod(nameof(MultiplayerLevelLoader.LoadLevel), allBindingFlags, null, parameters, Array.Empty<ParameterModifier>());
                HarmonyMethod prefix = new HarmonyMethod(typeof(MultiplayerLevelLoader_LoadLevel).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                MultiplayerLevelLoader_LoadLevel = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return MultiplayerLevelLoader_LoadLevel;
        }

        private static HarmonyPatchInfo GetSetPlayerLevelPatch()
        {
            if (SetPlayerLevelPatch == null)
            {
                MethodInfo original = typeof(LobbyPlayersDataModel).GetMethod("HandleMenuRpcManagerSelectedBeatmap", allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(SetPlayerLevelPatch).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                SetPlayerLevelPatch = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return SetPlayerLevelPatch;
        }

        private static HarmonyPatchInfo GetSetLocalPlayerLevelPatch()
        {
            if (SetLocalPlayerLevelPatch == null)
            {
                MethodInfo original = typeof(LobbyPlayersDataModel).GetMethod("SetLocalPlayerBeatmapLevel", allBindingFlags);
                HarmonyMethod prefix = new HarmonyMethod(typeof(SetLocalPlayerLevelPatch).GetMethod("Prefix", allBindingFlags));
                HarmonyMethod? postfix = null;
                SetLocalPlayerLevelPatch = new HarmonyPatchInfo(Harmony, original, prefix, postfix);
            }
            return SetLocalPlayerLevelPatch;
        }
    }
}
