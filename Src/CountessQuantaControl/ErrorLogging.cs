// Brad Pitney
// ECE 579
// Winter 2014

// ErrorLogging provides timestamped, severity-based log messages
// that can be displayed in the GUI.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountessQuantaControl
{
    public static class ErrorLogging
    {
        public enum LoggingLevel
        {
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 3
        }

        private class LogEntry
        {
            public LoggingLevel loggingLevel;
            public DateTime timestamp;
            public string logMessage;

            public LogEntry(LoggingLevel loggingLevel, DateTime timestamp, string logMessage)
            {
                this.loggingLevel = loggingLevel;
                this.timestamp = timestamp;
                this.logMessage = logMessage;
            }
        }

        static Queue<LogEntry> logQueue = new Queue<LogEntry>();
        static LoggingLevel loggingLevel = LoggingLevel.Info;
        static long logCountLimit = 100;
        static bool newLogMessageAvailable = false;
        static Object messageLock = new Object();

        // Check if the message severity is within the selected level.
        private static bool IsWithinLevel(LoggingLevel testLevel)
        {
            return (testLevel <= loggingLevel);
        }

        // Check if a new message has been added to the log since it 
        // was last read.
        public static bool NewLogMessageAvailable()
        {
            lock (messageLock)
            {
                return newLogMessageAvailable;
            }
        }

        // Create and return a string from the log entries.
        public static string GetLogString()
        {
            lock (messageLock)
            {
                string logString = "";

                foreach (LogEntry logEntry in logQueue)
                {
                    logString += logEntry.timestamp + " " + logEntry.logMessage + "\n";
                }

                newLogMessageAvailable = false;

                return logString;
            }
        }

        // Called from anywhere to add a new message to the log.
        public static void AddMessage(LoggingLevel logLevel, string logMessage)
        {
            if (IsWithinLevel(logLevel))
            {
                LogEntry logEntry = new LogEntry(logLevel, DateTime.Now, logMessage);

                lock (messageLock)
                {
                    logQueue.Enqueue(logEntry);

                    if (logQueue.Count > logCountLimit)
                    {
                        logQueue.Dequeue();
                    }

                    newLogMessageAvailable = true;
                }
            }
        }
    }
}
