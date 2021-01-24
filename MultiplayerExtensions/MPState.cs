using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions
{
    public static class MPState
    {
        private static MasterServerInfo _masterServerEndPoint = new MasterServerInfo("localhost", 2328);
        /// <summary>
        /// The current known Master Server.
        /// </summary>
        public static MasterServerInfo CurrentMasterServer
        {
            get => _masterServerEndPoint;
            internal set
            {
                if (_masterServerEndPoint == value)
                    return;
                _masterServerEndPoint = value;
                Plugin.Log?.Debug($"Updated MasterServer to '{value}'");
            }
        }

        private static string? _lastRoomCode;
        /// <summary>
        /// The last room code that was set.
        /// </summary>
        public static string? LastRoomCode
        {
            get => _lastRoomCode;
            internal set
            {
                if (_lastRoomCode == value)
                    return;
                _lastRoomCode = value;
                Plugin.Log?.Debug($"Updated room code to '{value}'");
            }
        }

        private static MultiplayerGameState? _currentGameState;
        /// <summary>
        /// The current multiplayer game state.
        /// </summary>
        public static MultiplayerGameState? CurrentGameState
        {
            get => _currentGameState;
            internal set
            {
                if (_currentGameState == value)
                    return;
                _currentGameState = value;
                Plugin.Log?.Debug($"Updated game state to '{value}'");
            }
        }

        private static MultiplayerGameType? _currentGameType;
        /// <summary>
        /// The current multiplayer game type.
        /// </summary>
        public static MultiplayerGameType? CurrentGameType
        {
            get => _currentGameType;
            internal set
            {
                if (_currentGameType == value)
                    return;
                _currentGameType = value;
                Plugin.Log?.Debug($"Updated game type to '{value}'");
            }
        }

        private static bool _customSongsEnabled;
        /// <summary>
        /// Whether custom songs are enabled in the current lobby.
        /// </summary>
        public static bool CustomSongsEnabled
        {
            get => _customSongsEnabled;
            internal set
            {
                if (_customSongsEnabled == value)
                    return;
                _customSongsEnabled = value;
                Plugin.Log?.Debug($"Update custom songs to '{value}'");
            }
        }
    }
}
