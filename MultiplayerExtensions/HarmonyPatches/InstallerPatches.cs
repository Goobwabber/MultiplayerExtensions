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
        private static readonly MethodInfo _overrideAttacher = SymbolExtensions.GetMethodInfo(() => PlayerDataModelAttacher(null!));
        private static readonly MethodInfo _originalMethod = _rootMethod.MakeGenericMethod(new Type[] { typeof(LobbyPlayersDataModel) });

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].Calls(_originalMethod))
                    {
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _overrideAttacher);
                        codes[i] = newCode;
                        break;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        private static FromBinderNonGeneric PlayerDataModelAttacher(ConcreteBinderNonGeneric contract)
        {
            return contract.To<PlayersDataModelStub>();
        }
    }
}
