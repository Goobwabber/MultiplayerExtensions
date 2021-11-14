using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(ConnectedPlayerManager), "OnNetworkReceive",
    new Type[] { typeof(IConnection), typeof(NetDataReader), typeof(DeliveryMethod) })]
    class PacketErrorLoggingPatch
    {
        private static readonly MethodInfo _exceptionLogger = SymbolExtensions.GetMethodInfo(() => ExceptionLogger((IConnectedPlayer)null!, (Exception)null!));
        private static ConcurrentDictionary<string, PlayerExceptionTracker> PlayerExceptions = new ConcurrentDictionary<string, PlayerExceptionTracker>();
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            if (_exceptionLogger == null)
                Plugin.Log?.Error($"Couldn't find _exceptionLogger");
            LocalBuilder? localException = gen.DeclareLocal(typeof(Exception));
            localException.SetLocalSymInfo("ex");
            foreach (CodeInstruction? code in instructions)
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
            //Plugin.Log?.Info($"{c.opcode} | {operandString}");
        }

        private static void ExceptionLogger(IConnectedPlayer p, Exception ex)
        {
            var tracker = PlayerExceptions.GetOrAdd(p.userId, new PlayerExceptionTracker());
            tracker.MaybeLogException(p, ex);
        }

        private class PlayerExceptionTracker
        {
            private ConcurrentDictionary<string, int> Exceptions = new ConcurrentDictionary<string, int>();

            public void MaybeLogException(IConnectedPlayer p, Exception ex)
            {
                int exCount = Exceptions.AddOrUpdate(ex.Message, 0, (msg, old) => old + 1);
                if (exCount % 20 == 0)
                {
                    Plugin.Log?.Warn($"An exception was thrown processing a packet from player '{p?.userName ?? "<NULL>"}|{p?.userId ?? " < NULL > "}' ({exCount + 1}): {ex.Message}");
                    Plugin.Log?.Debug(ex);
                }
            }
        }
    }
}
