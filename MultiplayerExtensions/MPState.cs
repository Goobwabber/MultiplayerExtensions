namespace MultiplayerExtensions
{
    public static class MPState
    {
        private static MasterServerInfo _masterServerEndPoint = new MasterServerInfo("localhost", 2328, "");
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

        private static MultiplayerGameState? _currentGameState = MultiplayerGameState.None;
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

        private static MultiplayerGameType? _currentGameType = MultiplayerGameType.None;
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
                Plugin.Log?.Debug($"Updated custom songs to '{value}'");
            }
        }

        private static bool _freeModEnabled;
        /// <summary>
        /// Whether free mod is enabled in the current lobby.
        /// </summary>
        public static bool FreeModEnabled
        {
            get => _freeModEnabled;
            internal set
            {
                if (_freeModEnabled == value)
                    return;
                _freeModEnabled = value;
                Plugin.Log?.Debug($"Updated free mod to '{value}'");
            }
        }

        private static bool _easterEggsEnabled = true;
        /// <summary>
        /// Whether easter eggs in multiplayer are enabled.
        /// </summary>
        public static bool EasterEggsEnabled
        {
            get => _easterEggsEnabled;
            internal set
            {
                if (_easterEggsEnabled == value)
                    return;
                _easterEggsEnabled = value;
                Plugin.Log?.Debug($"Easter Eggs {(value ? "enabled" : "disabled")}.");
            }
        }

        private static bool _localPlayerIsHost = false;
        /// <summary>
        /// Whether the local player is the lobby host.
        /// </summary>
        public static bool LocalPlayerIsHost
        {
            get => _localPlayerIsHost;
            internal set
            {
                if (_localPlayerIsHost == value)
                    return;
                _localPlayerIsHost = value;
                Plugin.Log?.Debug($"Local player is{(_localPlayerIsHost ? " " : " not ")}host.");
            }
        }
    }
}
