using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiplayerExtensions.Packets;
using System;

namespace MultiplayerExtensionsTests.Registry_Tests
{
    [TestClass]
    public class Registry_Tests
    {
        [TestMethod]
        public void RegistryType()
        {
            Registry<Type> registry = new Registry<Type>(256);

            registry.Register(typeof(string));
            registry.Register(typeof(int));
            registry.Register(typeof(float));

            Assert.AreEqual(registry.IndexOf(typeof(string)), 0);
            Assert.AreEqual(registry.IndexOf(typeof(int)), 1);
            Assert.AreEqual(registry.IndexOf(typeof(float)), 2);

            Assert.AreEqual(registry[0], typeof(string));
            Assert.AreEqual(registry[1], typeof(int));
            Assert.AreEqual(registry[2], typeof(float));
        }
    }
}
