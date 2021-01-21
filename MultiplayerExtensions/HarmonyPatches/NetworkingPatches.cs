using HarmonyLib;

namespace MultiplayerExtensions.HarmonyPatches
{
	/// <summary>
	/// For retrieving the currently used Master Server.
	/// </summary>
	[HarmonyPatch(typeof(NetworkConfigSO), "masterServerEndPoint", MethodType.Getter)]
	internal class GetMasterServerEndPointPatch
	{
		[HarmonyAfter("mod.serverbrowser")]
		[HarmonyPriority(Priority.Last)]
		internal static void Postfix(NetworkConfigSO __instance, ref MasterServerEndPoint __result)
		{
			if (__result != null)
			{
				MasterServerInfo info = new MasterServerInfo(__result);
				if (MPState.CurrentMasterServer.Equals(info))
					return;
				MPState.CurrentMasterServer = info;
				MPEvents.RaiseMasterServerChanged(__instance, info);
			}
		}
	}

	[HarmonyPatch(typeof(MultiplayerSettingsPanelController), "SetLobbyCode", MethodType.Normal)]
	internal class SetLobbyCodePatch
	{
		[HarmonyAfter("mod.serverbrowser")]
		[HarmonyPriority(Priority.Last)]
		public static void Postfix(MultiplayerSettingsPanelController __instance, string code)
		{
			if (code == MPState.LastRoomCode)
				return;
			MPState.LastRoomCode = code;
			MPEvents.RaiseRoomCodeChanged(__instance, code);
		}
	}
}
