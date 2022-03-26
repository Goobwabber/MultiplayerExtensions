using HarmonyLib;
using MultiplayerExtensions.Environments;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MultiplayerExtensions.Patchers
{
    public class AvatarPlacePatcher : IAffinity
    {
        private readonly MenuEnvironmentManager _environmentManager;
        private readonly SiraLog _logger;

        internal AvatarPlacePatcher(
            MenuEnvironmentManager environmentManager,
            SiraLog logger)
        {
            _environmentManager = environmentManager;
            _logger = logger;
        }

        private static readonly MethodInfo _addMethod = typeof(List<MultiplayerLobbyAvatarPlace>).GetMethod(nameof(List<MultiplayerLobbyAvatarPlace>.Add));
        private static readonly MethodInfo _setupAvatarPlaceMethod = SymbolExtensions.GetMethodInfo(() => SetupAvatarPlace(null!, 0));

        [AffinityTranspiler]
        [AffinityPatch(typeof(MultiplayerLobbyAvatarPlaceManager), nameof(MultiplayerLobbyAvatarPlaceManager.SpawnAllPlaces))]
        private IEnumerable<CodeInstruction> SpawnAllPlaces(IEnumerable<CodeInstruction> instructions) =>
            new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addMethod))
                .Insert(new CodeInstruction(OpCodes.Ldloc_3), new CodeInstruction(OpCodes.Callvirt, _setupAvatarPlaceMethod))
                .InstructionEnumeration();

        private static MultiplayerLobbyAvatarPlace SetupAvatarPlace(MultiplayerLobbyAvatarPlace avatarPlace, int sortIndex)
        {
            avatarPlace.gameObject.GetComponent<MpexAvatarPlaceLighting>().SortIndex = sortIndex;
            return avatarPlace;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLobbyAvatarPlaceManager), nameof(MultiplayerLobbyAvatarPlaceManager.SpawnAllPlaces))]
        private void SpawnAllPlacesPrefix(ILobbyStateDataModel ____lobbyStateDataModel)
            => _environmentManager.transform.Find("MultiplayerLobbyEnvironment").Find("LobbyAvatarPlace").gameObject.GetComponent<MpexAvatarPlaceLighting>().SortIndex = ____lobbyStateDataModel.localPlayer.sortIndex;
    }
}
