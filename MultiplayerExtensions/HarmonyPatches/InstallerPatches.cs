using HarmonyLib;
using MultiplayerExtensions.OverrideClasses;
using MultiplayerExtensions.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zenject;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(LobbyDataModelInstaller), nameof(LobbyDataModelInstaller.InstallBindings))]
    class LobbyPlayersDataModelPatch
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
            return contract.To<PlayersDataModelStub>();
        }

        private static FromBinderNonGeneric GameStateControllerAttacher(ConcreteBinderNonGeneric contract)
        {
            return contract.To<GameStateControllerStub>();
        }
    }

    [HarmonyPatch(typeof(MultiplayerMenuInstaller), nameof(MultiplayerMenuInstaller.InstallBindings))]
    class LevelLoaderPatch
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
            return contract.Bind(typeof(MultiplayerLevelLoader), typeof(ITickable)).To<LevelLoaderStub>();
        }
    }

    [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
    internal class ConnectedPlayerInstallerPatch
    {
        private static readonly SemVer.Version _minVersionFreeMod = new SemVer.Version("0.4.6");

        internal static void Prefix(ref GameplayCoreInstaller __instance, ref IConnectedPlayer ____connectedPlayer, ref GameplayCoreSceneSetupData ____sceneSetupData)
        {
            var mib = __instance as MonoInstallerBase;
            var Container = SiraUtil.Accessors.GetDiContainer(ref mib);

            ExtendedPlayerManager exPlayerManager = Container.Resolve<ExtendedPlayerManager>();
            ExtendedPlayer? exPlayer = exPlayerManager.GetExtendedPlayer(____connectedPlayer);
            ExtendedPlayer? hostPlayer = exPlayerManager.GetExtendedPlayer(Container.Resolve<IMultiplayerSessionManager>().connectionOwner);

            GameplayModifiers? newModifiers;
            if (____connectedPlayer.HasState("modded") && MPState.FreeModEnabled && exPlayer?.mpexVersion >= _minVersionFreeMod)
                newModifiers = exPlayer.lastModifiers;
            else
                newModifiers = hostPlayer?.lastModifiers;
                
            if (newModifiers != null)
                ____sceneSetupData = new GameplayCoreSceneSetupData(
                  ____sceneSetupData.difficultyBeatmap,
                  ____sceneSetupData.previewBeatmapLevel,
                  newModifiers,
                  ____sceneSetupData.playerSpecificSettings,
                  ____sceneSetupData.practiceSettings,
                  ____sceneSetupData.useTestNoteCutSoundEffects,
                  ____sceneSetupData.environmentInfo,
                  ____sceneSetupData.colorScheme
                );
        }
    }
}
