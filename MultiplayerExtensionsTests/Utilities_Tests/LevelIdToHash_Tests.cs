using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiplayerExtensions.Utilities;
using System;

namespace MultiplayerExtensionsTests.Utilities_Tests
{
    [TestClass]
    public class LevelIdToHash_Tests
    {
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
