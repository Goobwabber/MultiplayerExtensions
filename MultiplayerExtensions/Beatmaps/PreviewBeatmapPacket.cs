using LiteNetLib.Utils;
using MultiplayerExtensions.OverrideClasses;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Beatmaps
{
    class PreviewBeatmapPacket : INetSerializable, IPoolablePacket
    {
        public string levelId;
        public string songName;
        public string songSubName;
        public string songAuthorName;
        public string levelAuthorName;
        public float beatsPerMinute;
        public float songDuration;

        public bool isDownloadable;
        public byte[] coverImage;

        public string characteristic;
        public BeatmapDifficulty difficulty;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(levelId);
            writer.Put(songName);
            writer.Put(songSubName);
            writer.Put(songAuthorName);
            writer.Put(levelAuthorName);
            writer.Put(beatsPerMinute);
            writer.Put(songDuration);

            writer.Put(isDownloadable);

            writer.Put(characteristic);
            writer.PutVarUInt((uint)difficulty);

            writer.PutBytesWithLength(coverImage);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.levelId = reader.GetString();
            this.songName = reader.GetString();
            this.songSubName = reader.GetString();
            this.songAuthorName = reader.GetString();
            this.levelAuthorName = reader.GetString();
            this.beatsPerMinute = reader.GetFloat();
            this.songDuration = reader.GetFloat();

            this.isDownloadable = reader.GetBool();

            this.characteristic = reader.GetString();
            this.difficulty = (BeatmapDifficulty)reader.GetVarUInt();

            int imageLength = reader.GetInt();
            reader.GetBytes(this.coverImage, imageLength);
        }

        static async public Task<PreviewBeatmapPacket> FromPreview(PreviewBeatmapStub preview, string characteristic, BeatmapDifficulty difficulty)
        {
            PreviewBeatmapPacket packet = new PreviewBeatmapPacket();

            packet.levelId = preview.levelID;
            packet.songName = preview.songName;
            packet.songSubName = preview.songSubName;
            packet.songAuthorName = preview.songAuthorName;
            packet.levelAuthorName = preview.levelAuthorName;
            packet.beatsPerMinute = preview.beatsPerMinute;
            packet.songDuration = preview.songDuration;

            packet.isDownloadable = await preview.isDownloadable;
            packet.coverImage = await preview.GetRawCoverAsync(CancellationToken.None);

            packet.characteristic = characteristic;
            packet.difficulty = difficulty;

            return packet;
        }

        public void Release()
        {
            ThreadStaticPacketPool<PreviewBeatmapPacket>.pool.Release(this);
        }
    }
}
