using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace MultiplayerExtensions.HarmonyPatches
{
	/// <summary>
	/// For retrieving the currently used Master Server.
	/// </summary>
	[HarmonyPatch(typeof(NetworkConfigSO), "masterServerEndPoint", MethodType.Getter)]
	internal class GetMasterServerEndPointPatch
	{
		[HarmonyAfter("mod.serverbrowser", "com.Python.BeatTogether")]
		[HarmonyPriority(Priority.Last)]
		internal static void Postfix(NetworkConfigSO __instance, ref MasterServerEndPoint __result)
		{
			if (MPState.CurrentMasterServer.Equals(__result))
				return;
			MPState.CurrentMasterServer = new MasterServerInfo(__result.hostName, __result.port, __instance.masterServerStatusUrl);
			MPEvents.RaiseMasterServerChanged(__instance, MPState.CurrentMasterServer);
		}
	}

	/// <summary>
	/// For retrieving the Lobby code.
	/// </summary>
	[HarmonyPatch(typeof(MultiplayerSettingsPanelController), "SetLobbyCode", MethodType.Normal)]
	internal class SetLobbyCodePatch
	{ 
		[HarmonyAfter("mod.serverbrowser", "com.Python.BeatTogether")]
		[HarmonyPriority(Priority.Last)]
		internal static void Postfix(MultiplayerSettingsPanelController __instance, string code)
		{
			if (code == MPState.LastRoomCode)
				return;
			MPState.LastRoomCode = code;
			MPEvents.RaiseRoomCodeChanged(__instance, code);
		}
	}

	[HarmonyPatch(typeof(ConnectedPlayerManager), "FlushReliableQueue", MethodType.Normal)]
	internal class UpdateReliableFrequencyPatch
    {
		private static float nextTime = 0f;
		private static float frequency = 0.1f;

		internal static bool Prefix()
		{
			if (Time.time > nextTime)
			{
				nextTime = Time.time + frequency;
				return true;
			}
			return false;
		}
    }

	[HarmonyPatch(typeof(ConnectedPlayerManager), "PollUpdate", MethodType.Normal)]
	internal class UpdateUnreliableFrequencyPatch
	{
		private static readonly MethodInfo _updateUnreliableMethod = typeof(ConnectedPlayerManager).GetMethod("FlushUnreliableQueue", BindingFlags.NonPublic | BindingFlags.Instance);

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = instructions.ToList();
			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].Calls(_updateUnreliableMethod))
                {
					codes.RemoveRange(i, 2);
                }
			}

			return codes.AsEnumerable();
		}
	}

	// TODO: Make this work with harmony manager
	[HarmonyPatch(typeof(ConnectedPlayerManager), "SendUnreliable", MethodType.Normal)]
	internal class RemoveByteLimitPatch
    {
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = instructions.ToList();
			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Ble_S)
				{
					codes[i] = new CodeInstruction(OpCodes.Br_S, codes[i].operand);
				}
			}
			return codes.AsEnumerable();
		}
	}
}
