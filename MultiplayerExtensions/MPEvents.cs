using System;
using System.Collections.Generic;
using MultiplayerExtensions.Utilities;

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

        /// <summary>
        /// Raised when a player selects a beatmap.
        /// </summary>
        public static event EventHandler<SelectedBeatmapEventArgs>? BeatmapSelected;

        /// <summary>
        /// Raised after the lobby menu environment finishes loading in <see cref="MultiplayerLobbyController.ActivateMultiplayerLobby()"/>
        /// </summary>
        public static event EventHandler? LobbyEnvironmentLoaded;

        /// <summary>
        /// Raised when the game state changes.
        /// </summary>
        public static event EventHandler<MultiplayerGameState>? GameStateChanged;

        /// <summary>
        /// Raised when the host toggles custom songs.
        /// </summary>
        public static event EventHandler<bool>? CustomSongsChanged;

        /// <summary>
        /// Raised when the host toggles free mod.
        /// </summary>
        public static event EventHandler<bool>? FreeModChanged;

        internal static void RaiseMasterServerChanged(object sender, MasterServerInfo info)
            => MasterServerChanged?.RaiseEventSafe(sender, info, nameof(MasterServerChanged));
        internal static void RaiseRoomCodeChanged(object sender, string code) 
            =>  RoomCodeChanged.RaiseEventSafe(sender, code, nameof(RoomCodeChanged));
        internal static void RaiseBeatmapSelected(object sender, SelectedBeatmapEventArgs args) 
            => BeatmapSelected.RaiseEventSafe(sender, args, nameof(BeatmapSelected));
        internal static void RaiseLobbyEnvironmentLoaded(object sender)
            => LobbyEnvironmentLoaded.RaiseEventSafe(sender, nameof(LobbyEnvironmentLoaded));
        internal static void RaiseGameStateChanged(object sender, MultiplayerGameState state)
            => GameStateChanged.RaiseEventSafe(sender, state, nameof(GameStateChanged));
        internal static void RaiseCustomSongsChanged(object sender, bool state)
            => CustomSongsChanged.RaiseEventSafe(sender, state, nameof(CustomSongsChanged));
        internal static void RaiseFreeModChanged(object sender, bool state)
            => FreeModChanged.RaiseEventSafe(sender, state, nameof(FreeModChanged));
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
        public readonly string statusURL;
        public readonly bool isOfficial => hostname.Contains("beatsaber.com");

        public MasterServerInfo(string hostname, int port, string statusURL)
        {
            this.hostname = hostname;
            this.port = port;
            this.statusURL = statusURL;
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

        public bool Equals(string status)
        {
            return status == statusURL;
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
