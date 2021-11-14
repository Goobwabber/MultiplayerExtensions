using HarmonyLib;
using IPA.Utilities;
using MultiplayerExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zenject;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(LobbyDataModelInstaller), nameof(LobbyDataModelInstaller.InstallBindings))]
    internal class LobbyPlayersDataModelPatch
    {
        private static readonly MethodInfo _rootMethod = typeof(ConcreteBinderNonGeneric).GetMethod(nameof(ConcreteBinderNonGeneric.To), Array.Empty<Type>());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly MethodInfo _playersDataModelAttacher = SymbolExtensions.GetMethodInfo(() => PlayerDataModelAttacher(null));
        private static readonly MethodInfo _gameStateControllerAttacher = SymbolExtensions.GetMethodInfo(() => GameStateControllerAttacher(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        private static readonly MethodInfo _playersDataModelMethod = _rootMethod.MakeGenericMethod(new Type[] { typeof(LobbyPlayersDataModel) });
        private static readonly MethodInfo _gameStateControllerMethod = _rootMethod.MakeGenericMethod(new Type[] { typeof(LobbyGameStateController) });

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].Calls(_playersDataModelMethod))
                    {
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _playersDataModelAttacher);
                        codes[i] = newCode;
                    }

                    if (codes[i].Calls(_gameStateControllerMethod))
                    {
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _gameStateControllerAttacher);
                        codes[i] = newCode;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        private static FromBinderNonGeneric PlayerDataModelAttacher(ConcreteBinderNonGeneric contract)
        {
            return contract.To<ExtendedPlayersDataModel>();
        }

        private static FromBinderNonGeneric GameStateControllerAttacher(ConcreteBinderNonGeneric contract)
        {
            return contract.To<ExtendedGameStateController>();
        }
    }

    [HarmonyPatch(typeof(MainSystemInit), nameof(MainSystemInit.InstallBindings), MethodType.Normal)]
    internal class EntitlementCheckerPatch
    {
        private static readonly MethodInfo _rootMethod = typeof(FromBinder).GetMethod(nameof(FromBinder.FromComponentInNewPrefab), new[] { typeof(UnityEngine.Object) });
        private static readonly FieldInfo _sessionManagerPrefab = typeof(MainSystemInit).GetField("_multiplayerSessionManagerPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _entitlementCheckerPrefab = typeof(MainSystemInit).GetField("_networkPlayerEntitlementCheckerPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly MethodInfo _sessionManagerAttacher = SymbolExtensions.GetMethodInfo(() => SessionManagerAttacher(null, null));
        private static readonly MethodInfo _entitlementCheckerAttacher = SymbolExtensions.GetMethodInfo(() => EntitlementCheckerAttacher(null, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(_sessionManagerPrefab))
				{
                    if (codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].Calls(_rootMethod))
					{
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _sessionManagerAttacher);
                        codes[i + 1] = newCode;
                    }
				}

                if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(_entitlementCheckerPrefab))
				{
                    if (codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].Calls(_rootMethod))
                    {
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _entitlementCheckerAttacher);
                        codes[i + 1] = newCode;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        private static ScopeConcreteIdArgConditionCopyNonLazyBinder SessionManagerAttacher(ConcreteIdBinderGeneric<IMultiplayerSessionManager> contract, UnityEngine.Object prefab)
        {
            return contract.To<ExtendedSessionManager>().FromNewComponentOnRoot();
        }

        private static ScopeConcreteIdArgConditionCopyNonLazyBinder EntitlementCheckerAttacher(ConcreteIdBinderGeneric<NetworkPlayerEntitlementChecker> contract, UnityEngine.Object prefab)
        {
            return contract.To<ExtendedEntitlementChecker>().FromNewComponentOnRoot();
        }
    }

    [HarmonyPatch(typeof(MultiplayerMenuInstaller), nameof(MultiplayerMenuInstaller.InstallBindings))]
    internal class LevelLoaderPatch
    {
        private static readonly MethodInfo _rootMethod = typeof(DiContainer).GetMethod(nameof(DiContainer.BindInterfacesAndSelfTo), Array.Empty<Type>());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly MethodInfo _levelLoaderAttacher = SymbolExtensions.GetMethodInfo(() => LevelLoaderAttacher(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly MethodInfo _levelLoaderMethod = _rootMethod.MakeGenericMethod(new Type[] { typeof(MultiplayerLevelLoader) });

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].Calls(_levelLoaderMethod))
                {
                    CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _levelLoaderAttacher);
                    codes[i] = newCode;
                }
            }

            return codes.AsEnumerable();
        }

        private static FromBinderNonGeneric LevelLoaderAttacher(DiContainer contract)
        {
            return contract.Bind(typeof(MultiplayerLevelLoader), typeof(ITickable)).To<ExtendedLevelLoader>();
        }
    }

    [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
    internal class ConnectedPlayerInstallerPatch
    {
        internal static void Prefix(ref GameplayCoreInstaller __instance, ref IConnectedPlayer ____connectedPlayer, ref GameplayCoreSceneSetupData ____sceneSetupData)
        {
            ____sceneSetupData = new GameplayCoreSceneSetupData(
                ____sceneSetupData.difficultyBeatmap,
                ____sceneSetupData.previewBeatmapLevel,
                ____sceneSetupData.gameplayModifiers.CopyWith(zenMode: (____sceneSetupData.gameplayModifiers.zenMode || Plugin.Config.LagReducer)),
                ____sceneSetupData.playerSpecificSettings,
                ____sceneSetupData.practiceSettings,
                ____sceneSetupData.useTestNoteCutSoundEffects,
                ____sceneSetupData.environmentInfo,
                ____sceneSetupData.colorScheme
            );
        }
    }
}
