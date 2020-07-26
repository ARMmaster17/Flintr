using Flintr_Runner.Logging;
using NUnit.Framework;
using System;
using System.IO;

namespace Flintr_Runner_Test
{
    public class UnitLoggerTests
    {
        private StringWriter stringWriter;
        private Logger logger;
        private const string TEST_MESSAGE = "test message";
        private const string TEST_RUNNER = "TESTER";
        private const string TEST_JOB = "2";

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
            logger.Debug(TEST_RUNNER, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[DEBUG]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
        }

        [Test]
        public void TestLoggerDebugDetail()
        {
            logger.Debug(TEST_RUNNER, TEST_JOB, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[DEBUG]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
            Assert.That(output.Contains(TEST_JOB));
        }

        [Test]
        public void TestLoggerMsg()
        {
            logger.Msg(TEST_RUNNER, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[MESSAGE]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
        }

        [Test]
        public void TestLoggerMsgDetail()
        {
            logger.Msg(TEST_RUNNER, TEST_JOB, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[MESSAGE]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
            Assert.That(output.Contains(TEST_JOB));
        }

        [Test]
        public void TestLoggerWarning()
        {
            logger.Warning(TEST_RUNNER, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[WARNING]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
        }

        [Test]
        public void TestLoggerWarningDetail()
        {
            logger.Warning(TEST_RUNNER, TEST_JOB, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[WARNING]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
            Assert.That(output.Contains(TEST_JOB));
        }

        [Test]
        public void TestLoggerError()
        {
            logger.Error(TEST_RUNNER, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[ERROR]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
        }

        [Test]
        public void TestLoggerErrorDetail()
        {
            logger.Error(TEST_RUNNER, TEST_JOB, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[ERROR]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
            Assert.That(output.Contains(TEST_JOB));
        }

        [Test]
        public void TestLoggerCritical()
        {
            logger.Critical(TEST_RUNNER, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[CRITICAL]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
        }

        [Test]
        public void TestLoggerCriticalDetail()
        {
            logger.Critical(TEST_RUNNER, TEST_JOB, TEST_MESSAGE);
            string output = stringWriter.ToString();
            Assert.That(output.Contains("[CRITICAL]"));
            Assert.That(output.Contains(TEST_MESSAGE));
            Assert.That(output.Contains(TEST_RUNNER));
            Assert.That(output.Contains(TEST_JOB));
        }

        [Test]
        public void TestLoggerErrorStackTrace()
        {
            Exception a = new IOException("An error of type A occurred.");
            Exception b = new ArgumentException("An error of type B occurred.", a);

            logger.ErrorStackTrace(TEST_RUNNER, b);
            string output = stringWriter.ToString();

            Assert.That(output.Contains("IOException An error of type A occurred."));
            Assert.That(output.Contains("ArgumentException An error of type B occurred."));
            Assert.That(output.Contains("[ERROR]"));
            Assert.That(output.Contains(TEST_RUNNER));
        }

        [Test]
        public void TestLoggerErrorStackTraceDetail()
        {
            Exception a = new IOException("An error of type A occurred.");
            Exception b = new ArgumentException("An error of type B occurred.", a);

            logger.ErrorStackTrace(TEST_RUNNER, TEST_JOB, b);
            string output = stringWriter.ToString();

            Assert.That(output.Contains("IOException An error of type A occurred."));
            Assert.That(output.Contains("ArgumentException An error of type B occurred."));
            Assert.That(output.Contains("[ERROR]"));
            Assert.That(output.Contains(TEST_RUNNER));
            Assert.That(output.Contains(TEST_JOB));
        }
    }
}