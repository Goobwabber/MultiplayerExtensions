using HarmonyLib;
using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MultiplayerExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerPlayerPlacement), "GetAngleBetweenPlayersWithEvenAdjustment", MethodType.Normal)]
    class PlayerPlacementAnglePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

    [HarmonyPatch(typeof(CreateServerFormController), "formData", MethodType.Getter)]
    class IncreaseMaxPlayersClampPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].OperandIs(5))
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_R4, 10f);
                }
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(CreateServerFormController), "Setup", MethodType.Normal)]
    class IncreaseMaxPlayersPatch
    {
        static void Prefix(CreateServerFormController __instance)
        {
            FormattedFloatListSettingsController serverForm = __instance.GetField<FormattedFloatListSettingsController, CreateServerFormController>("_maxPlayersList");
            serverForm.values = new float[] { 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f };
        }
    }
}
