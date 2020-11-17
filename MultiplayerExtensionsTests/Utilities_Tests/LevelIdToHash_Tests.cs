using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiplayerExtensions.Utilities;
using System;

namespace MultiplayerExtensionsTests.Utilities_Tests
{
    [TestClass]
    public class LevelIdToHash_Tests
    {
        [TestMethod]
        public void Thing()
        {
            string path = @"J:\Oculus\Software\hyperbolic-magnetism-beat-saber\Beat Saber_Data\CustomLevels\4B48 (Camellia (Feat. Nanahira) - Can I Friend You On Bassbook Lol [Bassline Yatteru LOL] - RaxanZer\Camellia (Feat. Nanahira) - Can I Friend You On Bassbook Lol [Bassline Yatteru LOL].egg";
            int length = path.Length;
        }

        [TestMethod]
        public void EmptyString()
        {
            string expectedHash = null;
            string levelId = "";

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod]
        public void Null()
        {
            string expectedHash = null;
            string levelId = null;

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod]
        public void NormalCustomLevel()
        {
            string expectedHash = "D375405D047D6A2A4DD0F4D40D8DA77554F1F677";
            string levelId = $"custom_level_{expectedHash}";

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod]
        public void DuplicateCustomLevel()
        {
            string expectedHash = "D375405D047D6A2A4DD0F4D40D8DA77554F1F677";
            string levelId = $"custom_level_{expectedHash}_SomeSongFolder";

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod]
        public void WIP_CustomLevel()
        {
            string expectedHash = "D375405D047D6A2A4DD0F4D40D8DA77554F1F677";
            string levelId = $"custom_level_{expectedHash} WIP";

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod]
        public void BadLevelId()
        {
            string expectedHash = null;
            string levelId = $"Some_song_folder_with_underscores";

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod]
        public void OSTLevel()
        {
            string expectedHash = null;
            string levelId = $"100bills";

            string hash = Utils.LevelIdToHash(levelId);

            Assert.AreEqual(expectedHash, hash);
        }
    }
}
