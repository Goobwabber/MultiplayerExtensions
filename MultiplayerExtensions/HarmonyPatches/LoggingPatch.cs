using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(ConnectedPlayerManager), "OnNetworkReceive",
    new Type[] { typeof(IConnection), typeof(NetDataReader), typeof(DeliveryMethod) })]
    class LoggingPatch
    {
        private static readonly MethodInfo _exceptionLogger = SymbolExtensions.GetMethodInfo(() => ExceptionLogger(null));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Pop)
                {
                    CodeInstruction newCode = new CodeInstruction(OpCodes.Call, _exceptionLogger);
                    codes.Insert(i, newCode);
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static void ExceptionLogger(Exception ex)
        {
            Plugin.Log?.Warn($"Player was kicked: {ex}");
        }
    }
}
