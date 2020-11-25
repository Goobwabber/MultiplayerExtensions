using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly MethodInfo _exceptionLogger = SymbolExtensions.GetMethodInfo(() => ExceptionLogger((IConnectedPlayer)null!, (Exception)null!));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            Harmony.DEBUG = true;
            if (_exceptionLogger == null)
                Plugin.Log?.Error($"Couldn't find _exceptionLogger");
            else
                Plugin.Log?.Warn($"Applying transpiler.");
            var localException = gen.DeclareLocal(typeof(Exception));
            localException.SetLocalSymInfo("ex");
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Pop)
                {
                    CodeInstruction current = new CodeInstruction(OpCodes.Stloc, localException);
                    current.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock));
                    LogInstruction(current);
                    yield return current; // Store exception in local
                    current = new CodeInstruction(OpCodes.Ldloc_2); // load player
                    LogInstruction(current);
                    yield return current; // load player onto stack
                    current = new CodeInstruction(OpCodes.Ldloc, localException);
                    LogInstruction(current);
                    yield return current; // load exception onto stack
                    current = new CodeInstruction(OpCodes.Call, _exceptionLogger);
                    LogInstruction(current);
                    yield return current; // Calls logger
                }
                else
                {
                    LogInstruction(code);
                    yield return code;
                }

            }
            yield break;
        }

        [Conditional("DEBUG")]
        private static void LogInstruction(CodeInstruction c)
        {
            string? operandString = c.operand?.ToString();
            if (c.operand is System.Reflection.Emit.Label label)
            {
                operandString = $"({label.GetHashCode().ToString()}) {operandString}";
            }
            Plugin.Log?.Info($"{c.opcode} | {operandString}");
        }

        private static void ExceptionLogger(IConnectedPlayer p, Exception ex)
        {
            Plugin.Log?.Warn($"An exception was thrown processing a packet from player '{p?.userName ?? "<NULL>"}|{p?.userId ?? " < NULL > "}': {ex.Message}");
            Plugin.Log?.Debug(ex);
        }
    }
}
