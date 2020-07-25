using System;
using System.Threading;

namespace Flintr_Runner.Logging
{
    /// <summary>
    /// Manages thread-safe writing of formatted messages to console output.
    /// </summary>
    public class Logger
    {
        private readonly int logLevel;
        private bool outputLocked;
        private const ConsoleColor DEFAULT_CONSOLE_FOREGROUND_COLOR = ConsoleColor.White;
        private const string TAG_DEBUG = "DEBUG";
        private const string TAG_MSG = "MESSAGE";
        private const string TAG_WARNING = "WARNING";
        private const string TAG_ERROR = "ERROR";
        private const string TAG_CRITICAL = "CRITICAL";
        private const ConsoleColor COLOR_DEBUG = ConsoleColor.Gray;
        private const ConsoleColor COLOR_MSG = ConsoleColor.White;
        private const ConsoleColor COLOR_WARNING = ConsoleColor.Yellow;
        private const ConsoleColor COLOR_ERROR = ConsoleColor.Red;
        private const ConsoleColor COLOR_CRITICAL = ConsoleColor.DarkRed;
        private const int DEFAULT_LOG_LEVEL = 1;

        /// <summary>
        /// Initializes Logger with default settings.
        /// </summary>
        public Logger()
        {
            logLevel = DEFAULT_LOG_LEVEL;
            outputLocked = false;
        }

        /// <summary>
        /// Initializes a Logger with default settings and the specified logging level.
        /// </summary>
        /// <param name="logLevel">Max level of severity rating of messages to permit to be written to console output.</param>
        public Logger(int logLevel)
        {
            this.logLevel = logLevel;
            outputLocked = false;
        }

        /// <summary>
        /// Display a debug message. Shown when logLevel is 3 or greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        public void Debug(string runnerName, string message)
        {
            if (logLevel >= 3) writeMessage(TAG_DEBUG, message, COLOR_DEBUG, runnerName, null);
        }

        /// <summary>
        /// Display a debug message. Shown when logLevel is 3 or greater.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="jobName">Name of job/task that emitted this message.</param>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Debug(string runnerName, string jobName, string message)
        {
            if (logLevel >= 3) writeMessage(TAG_DEBUG, message, COLOR_DEBUG, runnerName, jobName);
        }

        /// <summary>
        /// Display a log message from an application message. Shown when logLevel is 2 or
        /// greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        public void Msg(string runnerName, string message)
        {
            if (logLevel >= 2) writeMessage(TAG_MSG, message, COLOR_MSG, runnerName, null);
        }

        /// <summary>
        /// Display a log message from an application message. Shown when logLevel is 2 or
        /// greater.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="jobName">Name of job/task that emitted this message.</param>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Msg(string runnerName, string jobName, string message)
        {
            if (logLevel >= 2) writeMessage(TAG_MSG, message, COLOR_MSG, runnerName, jobName);
        }

        /// <summary>
        /// Display a log message from an application warning. Shown when logLevel is 1 or
        /// greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        public void Warning(string runnerName, string message)
        {
            if (logLevel >= 1) writeMessage(TAG_WARNING, message, COLOR_WARNING, runnerName, null);
        }

        /// <summary>
        /// Display a log message from an application warning. Shown when logLevel is 1 or
        /// greater.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="jobName">Name of job/task that emitted this message.</param>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Warning(string runnerName, string jobName, string message)
        {
            if (logLevel >= 1) writeMessage(TAG_WARNING, message, COLOR_WARNING, runnerName, jobName);
        }

        /// <summary>
        /// Display a log message from a non-critical application error. Shown when logLevel
        /// is 0 or greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        public void Error(string runnerName, string message)
        {
            if (logLevel >= 0) writeMessage(TAG_ERROR, message, COLOR_ERROR, runnerName, null);
        }

        /// <summary>
        /// Display a log message from a non-critical application error. Shown when logLevel
        /// is 0 or greater.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="jobName">Name of job/task that emitted this message.</param>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Error(string runnerName, string jobName, string message)
        {
            if (logLevel >= 0) writeMessage(TAG_ERROR, message, COLOR_ERROR, runnerName, jobName);
        }

        /// <summary>
        /// Display log message from a critical application failure. Always shown regardless
        /// of the user-specified logLevel.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        public void Critical(string runnerName, string message)
        {
            writeMessage(TAG_CRITICAL, message, COLOR_CRITICAL, runnerName, null);
        }

        /// <summary>
        /// Display log message from a critical application failure. Always shown regardless
        /// of the user-specified logLevel.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="jobName">Name of job/task that emitted this message.</param>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Critical(string runnerName, string jobName, string message)
        {
            writeMessage(TAG_CRITICAL, message, COLOR_CRITICAL, runnerName, jobName);
        }

        /// <summary>
        /// Prints a stack trace of a non-critical error that occurred if the current session
        /// settings allow it.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="jobName">Name of job/task that emitted this message.</param>
        /// <param name="e">The runtime exception that was thrown.</param>
        public void ErrorStackTrace(string runnerName, string jobName, Exception e)
        {
            if (e == null) return;
            Error(runnerName, jobName, $"{e.GetType().Name} {e.Message}");
            ErrorStackTrace(runnerName, jobName, e.InnerException);
        }

        /// <summary>
        /// Prints a stack trace of a non-critical error that occurred if the current session
        /// settings allow it.
        /// </summary>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        /// <param name="e">The runtime exception that was thrown.</param>
        public void ErrorStackTrace(string runnerName, Exception e)
        {
            if (e == null) return;
            Error(runnerName, $"{e.GetType().Name} {e.Message}");
            ErrorStackTrace(runnerName, e.InnerException);
        }

        /// <summary>
        /// Internal method to write a message to the screen and log file (not implemented)
        /// with the given level, message, and color.
        /// </summary>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="message">Message to write.</param>
        /// <param name="color">Text color to use.</param>
        /// <param name="runnerName">Name of thread/runner that sent the message.</param>
        private void writeMessage(string level, string message, ConsoleColor color, string runnerName, string jobName)
        {
            // TODO: Replace with lock(){} keywords.
            while (outputLocked)
            {
                Thread.Sleep(100);
            }
            outputLocked = true;
            Console.ForegroundColor = color;
            if (jobName == null && runnerName == null) Console.WriteLine($"[{level}] {DateTime.Now} - {message}");
            else if (jobName == null) Console.WriteLine($"[{level}] {DateTime.Now} - [{runnerName}]: {message}");
            else Console.WriteLine($"[{level}] {DateTime.Now} - [{runnerName}/{jobName}]: {message}");
            Console.ForegroundColor = DEFAULT_CONSOLE_FOREGROUND_COLOR;
            outputLocked = false;
        }
    }
}