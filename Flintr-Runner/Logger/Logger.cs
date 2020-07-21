using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.Logger
{
    public class Logger
    {
        private int logLevel;
        private bool outputLocked;
        private static readonly ConsoleColor DEFAULT_CONSOLE_FOREGROUND_COLOR = ConsoleColor.White;
        private static readonly string TAG_DEBUG = "DEBUG";
        private static readonly string TAG_MSG = "MESSAGE";
        private static readonly string TAG_WARNING = "WARNING";
        private static readonly string TAG_ERROR = "ERROR";
        private static readonly string TAG_CRITICAL = "CRITICAL";
        private static readonly ConsoleColor COLOR_DEBUG = ConsoleColor.Gray;
        private static readonly ConsoleColor COLOR_MSG = ConsoleColor.White;
        private static readonly ConsoleColor COLOR_WARNING = ConsoleColor.Yellow;
        private static readonly ConsoleColor COLOR_ERROR = ConsoleColor.Red;
        private static readonly ConsoleColor COLOR_CRITICAL = ConsoleColor.DarkRed;

        public Logger()
        {
            logLevel = 0;
            outputLocked = false;
        }

        public Logger(int logLevel)
        {
            this.logLevel = logLevel;
            outputLocked = false;
        }

        public void SetLogLevel(int logLevel)
        {
            this.logLevel = logLevel;
        }

        /// <summary>
        /// Display a debug message. Shown when logLevel is 3 or greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Debug(string message)
        {
            if (logLevel >= 3) writeMessage(TAG_DEBUG, message, COLOR_DEBUG);
        }

        /// <summary>
        /// Display a log message from an application message. Shown when logLevel is 2 or
        /// greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Msg(string message)
        {
            if (logLevel >= 2) writeMessage(TAG_MSG, message, COLOR_MSG);
        }

        /// <summary>
        /// Display a log message from an application warning. Shown when logLevel is 1 or
        /// greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Warning(string message)
        {
            if (logLevel >= 1) writeMessage(TAG_WARNING, message, COLOR_WARNING);
        }

        /// <summary>
        /// Display a log message from a non-critical application error. Shown when logLevel
        /// is 0 or greater.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Error(string message)
        {
            if (logLevel >= 0) writeMessage(TAG_ERROR, message, COLOR_ERROR);
        }

        /// <summary>
        /// Display log message from a critical application failure. Always shown regardless
        /// of the user-specified logLevel.
        /// </summary>
        /// <param name="message">Message to display in console output and log file.</param>
        public void Critical(string message)
        {
            writeMessage(TAG_CRITICAL, message, COLOR_CRITICAL);
        }

        /// <summary>
        /// Internal method to write a message to the screen and log file (not implemented)
        /// with the given level, message, and color.
        /// </summary>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="message">Message to write.</param>
        /// <param name="color">Text color to use.</param>
        private void writeMessage(string level, string message, ConsoleColor color)
        {
            while (outputLocked) { }
            outputLocked = true;
            Console.ForegroundColor = color;
            Console.WriteLine("[{0}] {1} - {2}", level, DateTime.Now, message);
            Console.ForegroundColor = DEFAULT_CONSOLE_FOREGROUND_COLOR;
            outputLocked = false;
        }
    }
}