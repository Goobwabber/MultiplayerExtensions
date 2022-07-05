using SiraUtil.Affinity;
using System;

namespace MultiplayerExtensions.Environment
{
    public class MpexLevelEndActions : IAffinity, ILevelEndActions
    {
        public event Action levelFailedEvent = null!;
        public event Action levelFinishedEvent = null!;

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLocalActivePlayerFacade), "ReportPlayerDidFinish")]
        private void PlayerDidFinish() =>
            levelFinishedEvent?.Invoke();

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLocalActivePlayerFacade), "ReportPlayerNetworkDidFailed")]
        private void PlayerDidFail() =>
            levelFailedEvent?.Invoke();
    }
}
