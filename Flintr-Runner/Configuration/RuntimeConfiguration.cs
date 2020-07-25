using Flintr_Runner.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Flintr_Runner.Configuration
{
    /// <summary>
    /// Contains the active runtime configuration. In the future, this class will pull from
    /// settings files, environment variable, and command-line arguments in that order.
    /// </summary>
    public class RuntimeConfiguration
    {
        protected Logger logger;
        protected int logLevel;
        protected int workerThreads;
        protected IPAddress managerBindAddress;
        protected int managerConPort;
        protected int managerExecPort;
        protected IPAddress workerConnectComAddress;
        protected int workerConnectComPort;
        protected int workerDeadHeartbeatTimeout;

        public RuntimeConfiguration()
        {
            // Get the log level setting.
            logLevel = 3;
            logger = new Logger(logLevel);
            // Set the number of worker threads.
            workerThreads = 4;
            managerBindAddress = IPAddress.Parse("127.0.0.1");
            managerConPort = 4000;
            managerExecPort = 3999;
            workerConnectComAddress = IPAddress.Parse("127.0.0.1");
            workerConnectComPort = 4000;
            workerDeadHeartbeatTimeout = 5;
        }

        public Logger GetLoggerInstance()
        {
            return logger;
        }

        public int GetWorkerThreadCount()
        {
            return workerThreads;
        }

        public IPAddress GetManagerBindAddress()
        {
            return managerBindAddress;
        }

        public int GetManagerComPort()
        {
            return managerConPort;
        }

        public IPAddress GetWorkerConnectComAddress()
        {
            return workerConnectComAddress;
        }

        public int GetWorkerConnectComPort()
        {
            return workerConnectComPort;
        }

        public int GetWorkerHeartbeatTimeout()
        {
            return workerDeadHeartbeatTimeout;
        }

        public int GetManagerExecPort()
        {
            return managerExecPort;
        }
    }
}