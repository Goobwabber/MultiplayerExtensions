using HarmonyLib;
using MultiplayerExtensions.OverrideClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(LobbyDataModelInstaller), "InstallBindings")]
    class LobbyPlayersDataModelPatch
    {
        private static readonly MethodInfo _rootMethod = typeof(ConcreteBinderNonGeneric).GetMethod(nameof(ConcreteBinderNonGeneric.To), Array.Empty<Type>());

        private static readonly MethodInfo _playersDataModelAttacher = SymbolExtensions.GetMethodInfo(() => PlayerDataModelAttacher(null));
        private static readonly MethodInfo _gameStateControllerAttacher = SymbolExtensions.GetMethodInfo(() => GameStateControllerAttacher(null));

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
}
