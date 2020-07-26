using Flintr_lib.Communication;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;

namespace Flintr_Runner_Test
{
    [NonParallelizable]
    public class UnitTCPServerTests
    {
        private IPAddress address;
        private int port;

        [SetUp]
        public void Setup()
        {
            address = IPAddress.Parse("127.0.0.1");
            port = 4000;
        }

        [Test]
        public void TestDefaultInitializer()
        {
            TCPServer tcpServer = new TCPServer(address, port);
        }

        [Test]
        public void TestInvalidIPInitializer()
        {
            address = null;
            TCPServer tcpServer;
            ArgumentException ae = Assert.Throws<ArgumentException>(() => tcpServer = new TCPServer(address, port));
            Assert.AreEqual(ae.Message, "Binding IP address and port cannot be NULL.");
        }

        [Test]
        public void TestInvalidPortInitializer()
        {
            port = -20;
            TCPServer tcpServer;
            ArgumentException ae = Assert.Throws<ArgumentException>(() => tcpServer = new TCPServer(address, port));
            Assert.AreEqual(ae.Message, "Invalid IP address or bind port specified. Check values and try again.");
        }

        [Test]
        public void TestSocketCollisionInitializer()
        {
            port = 5000;
            TCPServer existingServer = new TCPServer(address, port);

            TCPServer tcpServer;
            IOException ae = Assert.Throws<IOException>(() => tcpServer = new TCPServer(address, port));
            Assert.AreEqual(ae.Message, "An error occurred while initializing a TCP listener.");
        }
    }
}