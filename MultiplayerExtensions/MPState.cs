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
            get { return _lastRoomCode; }
            set
            {
                if (_lastRoomCode == value)
                    return;
                _lastRoomCode = value;
                Plugin.Log?.Debug($"Updated room code to '{value}'");
            }
        }


    }
}
