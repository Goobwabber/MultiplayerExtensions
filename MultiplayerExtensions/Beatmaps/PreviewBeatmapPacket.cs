using LiteNetLib.Utils;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Beatmaps
{
    public class PreviewBeatmapPacket : INetSerializable, IPoolablePacket
    {
        // Basic Song Info/Metadata
        public string levelId = null!;
        public string levelHash = null!;
        public string songName = null!;
        public string songSubName = null!;
        public string songAuthorName = null!;
        public string levelAuthorName = null!;
        public float beatsPerMinute;
        public float songDuration;

        // Selection Info
        public string characteristic = null!;
        public BeatmapDifficulty difficulty;

        public PreviewBeatmapPacket() { }

        public PreviewBeatmapPacket(PreviewBeatmapStub preview, string characteristic, BeatmapDifficulty difficulty)
		{
            this.levelId = preview.levelID;
            this.levelHash = preview.levelHash;
            this.songName = preview.songName;
            this.songSubName = preview.songSubName;
            this.songAuthorName = preview.songAuthorName;
            this.levelAuthorName = preview.levelAuthorName;
            this.beatsPerMinute = preview.beatsPerMinute;
            this.songDuration = preview.songDuration;

            this.characteristic = characteristic;
            this.difficulty = difficulty;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.levelId);
            writer.Put(this.levelHash);
            writer.Put(this.songName);
            writer.Put(this.songSubName);
            writer.Put(this.songAuthorName);
            writer.Put(this.levelAuthorName);
            writer.Put(this.beatsPerMinute);
            writer.Put(this.songDuration);

            writer.Put(this.characteristic);
            writer.Put((uint)this.difficulty);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.levelId = reader.GetString();
            this.levelHash = reader.GetString();
            this.songName = reader.GetString();
            this.songSubName = reader.GetString();
            this.songAuthorName = reader.GetString();
            this.levelAuthorName = reader.GetString();
            this.beatsPerMinute = reader.GetFloat();
            this.songDuration = reader.GetFloat();

            this.characteristic = reader.GetString();
            this.difficulty = (BeatmapDifficulty)reader.GetUInt();
        }

        public void Release()
        {
            ThreadStaticPacketPool<PreviewBeatmapPacket>.pool.Release(this);
        }
    }
}
