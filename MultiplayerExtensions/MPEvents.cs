using System;
using System.Collections.Generic;
using MultiplayerExtensions.Utilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions
{
    public static class MPEvents
    {
        /// <summary>
        /// Raised when <see cref="NetworkConfigSO.masterServerEndPoint"/> is accessed and it's different from the last known value in <see cref="Plugin.CurrentMasterServer"/>
        /// </summary>
        public static event EventHandler<MasterServerInfo>? MasterServerChanged;

        /// <summary>
        /// Raised when the room code is set in <see cref="HostLobbySetupViewController.SetLobbyCode(string)"/>.
        /// </summary>
        public static event EventHandler<string>? RoomCodeChanged;

        public static event EventHandler<SelectedBeatmapEventArgs>? BeatmapSelected;

        internal static void RaiseMasterServerChanged(object sender, MasterServerInfo info)
            => MasterServerChanged?.RaiseEventSafe(sender, info, nameof(MasterServerChanged));
        internal static void RaiseRoomCodeChanged(object sender, string code) 
            =>  RoomCodeChanged.RaiseEventSafe(sender, code, nameof(RoomCodeChanged));
        internal static void RaiseBeatmapSelected(object sender, SelectedBeatmapEventArgs args) 
            => BeatmapSelected.RaiseEventSafe(sender, args, nameof(BeatmapSelected));
    }

    public class SelectedBeatmapEventArgs : EventArgs
    {
        public readonly string UserId;
        public readonly UserType UserType;
        public readonly string LevelId;
        public readonly BeatmapDifficulty BeatmapDifficulty;
        public readonly BeatmapCharacteristicSO? BeatmapCharacteristic;

        public SelectedBeatmapEventArgs(string userId, UserType userType, string levelId, 
            BeatmapDifficulty difficulty, BeatmapCharacteristicSO? characteristicSO)
            : this(userId, userType)
        {
            LevelId = levelId;
            BeatmapDifficulty = difficulty;
            BeatmapCharacteristic = characteristicSO;
        }

        public SelectedBeatmapEventArgs(string userId, UserType userType)
        {
            UserId = userId;
            UserType = userType;
            LevelId = string.Empty;
            BeatmapDifficulty = BeatmapDifficulty.Easy;
            BeatmapCharacteristic = null;
        }
    }
    [Flags]
    public enum UserType
    {
        None = 0,
        Local = 1 << 0,
        Host = 1 << 1
    }

    public struct MasterServerInfo : IEquatable<MasterServerInfo>, IEquatable<MasterServerEndPoint>
    {
        public readonly string hostname;
        public readonly int port;
        public MasterServerInfo(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }
        public MasterServerInfo(MasterServerEndPoint endPoint)
        {
            if (endPoint == null)
            {
                hostname = "localhost";
                port = 2328;
                return;
            }
            hostname = endPoint.hostName;
            port = endPoint.port;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return "INVALID";
            return $"{hostname}:{port}";
        }

        public bool Equals(MasterServerEndPoint endPoint)
        {
            if (endPoint == null)
                return false;
            return endPoint.hostName == hostname && endPoint.port == port;
        }

        public override bool Equals(object obj)
        {
            if (obj is MasterServerEndPoint endPoint)
                return Equals(endPoint);
            if (obj is MasterServerInfo info)
                return Equals(info);
            return false;
        }

        public bool Equals(MasterServerInfo other)
        {
            return hostname == other.hostname &&
                   port == other.port;
        }

        public override int GetHashCode()
        {
            int hashCode = -1042146062;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(hostname);
            hashCode = hashCode * -1521134295 + port.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MasterServerInfo left, MasterServerInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MasterServerInfo left, MasterServerInfo right)
        {
            return !(left == right);
        }
    }
}
