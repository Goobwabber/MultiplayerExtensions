using HarmonyLib;
using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerPlayerPlacement), "GetAngleBetweenPlayersWithEvenAdjustment", MethodType.Normal)]
    internal class PlayerPlacementAnglePatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool flag = true;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4)
                {
                    flag = false;
                }

                if (flag)
                {
                    codes.RemoveAt(0);
                    i--;
                }
            }
            return codes.AsEnumerable();
        }
    }
    
    [HarmonyPatch(typeof(MultiplayerLayoutProvider), "CalculateLayout", MethodType.Normal)]
    internal class PlayerLayoutSpotsCountPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool flag = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_1)
                {
                    flag = true;
                    continue;
                }

                if (codes[i].opcode == OpCodes.Add && flag)
                {
                    codes.RemoveAt(i);
                    codes.RemoveAt(i - 1);
                    break;
                }

                flag = false;
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(CreateServerFormController), "formData", MethodType.Getter)]
    internal class IncreaseMaxPlayersClampPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].OperandIs(5))
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_R4, (float)Plugin.Config.MaxPlayers);
                }
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(CreateServerFormController), "Setup", MethodType.Normal)]
    internal class IncreaseMaxPlayersPatch
    {
        internal static void Prefix(CreateServerFormController __instance, ref FormattedFloatListSettingsController ____maxPlayersList)
        {
            int maxPlayers = MPState.CurrentMasterServer.isOfficial ? 5 : Plugin.Config.MaxPlayers;
            float[] playerValues = Enumerable.Range(2, maxPlayers-1).Select(x => (float)x).ToArray();
            ____maxPlayersList.values = playerValues;
        }
    }
}
