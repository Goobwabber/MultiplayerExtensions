using HarmonyLib;

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
			MPState.CurrentMasterServer.SetEndPoint(__result);
			MPEvents.RaiseMasterServerChanged(__instance, MPState.CurrentMasterServer);
		}
	}

	/// <summary>
	/// For retrieving the currently used Master Server status URL.
	/// </summary>
	[HarmonyPatch(typeof(NetworkConfigSO), "masterServerStatusUrl", MethodType.Getter)]
	internal class GetMasterServerStatusUrlPatch
    {
		[HarmonyAfter("mod.serverbrowser", "com.Python.BeatTogether")]
		[HarmonyPriority(Priority.Last)]
		internal static void Postfix(NetworkConfigSO __instance, ref string __result)
        {
			if (MPState.CurrentMasterServer.Equals(__result))
				return;
			MPState.CurrentMasterServer.SetStatusURL(__result);
			MPEvents.RaiseMasterServerChanged(__instance, MPState.CurrentMasterServer);
        }
	}

	/// <summary>
	/// For retrieving the currently used Master Server Status URL.
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
}
