using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Avatars
{
    public class CustomAvatarData
    {
        public string hash;
        public float scale;
        public float floor;
        public bool pelvis;

        public CustomAvatarData() { }

        public CustomAvatarData(string hash, float scale, float floor, bool pelvis)
        {
            this.hash = hash;
            this.scale = scale;
            this.floor = floor;
            this.pelvis = pelvis;
        }

        public CustomAvatarData(CustomAvatarPacket packet)
        {
            hash = packet.avatarHash;
            scale = packet.avatarScale;
            floor = packet.avatarFloor;
            pelvis = packet.avatarPelvis;
        }

        public CustomAvatarPacket GetAvatarPacket()
        {
            return new CustomAvatarPacket().Init(hash, scale, floor, pelvis);
        }
    }
}
