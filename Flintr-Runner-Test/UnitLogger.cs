using Flintr_Runner.Logger;
using NUnit.Framework;
using System;
using System.IO;

namespace Flintr_Runner_Test
{
    public class UnitLoggerTests
    {
        private StringWriter stringWriter;
        private Logger logger;
        private static readonly string TEST_MESSAGE = "test message";

        [SetUp]
        public void Setup()
        {
            // Set up logger to write all levels of messages.
            logger = new Logger(3);
            // Set up a buffer to catch Console output.
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
        }

        [Test]
        public void TestLoggerDebug()
        {
            logger.Debug(TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[DEBUG]"));
            Assert.That(output.Contains(TEST_MESSAGE));
        }

        [Test]
        public void TestLoggerMsg()
        {
            logger.Msg(TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[MESSAGE]"));
            Assert.That(output.Contains(TEST_MESSAGE));
        }

        [Test]
        public void TestLoggerWarning()
        {
            logger.Warning(TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[WARNING]"));
            Assert.That(output.Contains(TEST_MESSAGE));
        }

        [Test]
        public void TestLoggerError()
        {
            logger.Error(TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[ERROR]"));
            Assert.That(output.Contains(TEST_MESSAGE));
        }

        [Test]
        public void TestLoggerCritical()
        {
            logger.Critical(TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[CRITICAL]"));
            Assert.That(output.Contains(TEST_MESSAGE));
        }
    }
}