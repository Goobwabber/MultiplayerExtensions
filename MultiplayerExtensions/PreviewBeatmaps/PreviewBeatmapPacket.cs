using LiteNetLib.Utils;
using MultiplayerExtensions.OverrideClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Beatmaps
{
    class PreviewBeatmapPacket : INetSerializable, IPoolablePacket
    {
        public static PacketPool<PreviewBeatmapPacket> pool
        {
            get
            {
                return ThreadStaticPacketPool<PreviewBeatmapPacket>.pool;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(levelId);
            writer.Put(songName);
            writer.Put(songSubName);
            writer.Put(songAuthorName);
            writer.Put(levelAuthorName);
            writer.Put(characteristic);
            writer.PutVarUInt((uint)difficulty);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.levelId = reader.GetString();
            this.songName = reader.GetString();
            this.songSubName = reader.GetString();
            this.songAuthorName = reader.GetString();
            this.levelAuthorName = reader.GetString();
            this.characteristic = reader.GetString();
            this.difficulty = (BeatmapDifficulty)reader.GetVarUInt();
        }

        public void Release()
        {
            PreviewBeatmapPacket.pool.Release(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="songName"></param>
        /// <param name="songSubName"></param>
        /// <param name="songAuthorName"></param>
        /// <param name="levelAuthorName"></param>
        /// <param name="characteristic"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PreviewBeatmapPacket Init(string levelId, string songName, string songSubName, string songAuthorName, string levelAuthorName, string characteristic, BeatmapDifficulty difficulty)
        {
            this.levelId = levelId ?? throw new ArgumentNullException(nameof(levelId));
            this.songName = songName;
            this.songSubName = songSubName;
            this.songAuthorName = songAuthorName;
            this.levelAuthorName = levelAuthorName;
            this.characteristic = characteristic;
            this.difficulty = difficulty;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preview"></param>
        /// <param name="characteristic"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public PreviewBeatmapPacket FromPreview(PreviewBeatmapStub preview, string characteristic, BeatmapDifficulty difficulty)
        {
            this.levelId = preview.levelID ?? throw new ArgumentException("PreviewBeatmapStub has a null levelID", nameof(preview));
            this.songName = preview.songName;
            this.songSubName = preview.songSubName;
            this.songAuthorName = preview.songAuthorName;
            this.levelAuthorName = preview.levelAuthorName;
            this.characteristic = characteristic;
            this.difficulty = difficulty;

            return this;
        }

        public string levelId = null!;
        public string? songName;
        public string? songSubName;
        public string? songAuthorName;
        public string? levelAuthorName;
        public string? characteristic;
        public BeatmapDifficulty difficulty;
    }
}
