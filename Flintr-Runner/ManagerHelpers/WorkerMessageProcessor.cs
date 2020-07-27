using Flintr_Runner.Configuration;
using Flintr_Runner.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.ManagerHelpers
{
    /// <summary>
    /// Handles messages sent from workers.
    /// </summary>
    public class WorkerMessageProcessor
    {
        private Logger sharedLogger;
        private WorkerRegistrationPool workerRegistrationPool;
        private DataStoreManager dataStoreManager;

        /// <summary>
        /// Default constructor with default settings.
        /// </summary>
        /// <param name="runtimeConfiguration">Active runtime configuration settings for current session.</param>
        /// <param name="workerRegistrationPool">Active pool of workers to pull information from.</param>
        public WorkerMessageProcessor(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool, DataStoreManager dataStoreManager)
        {
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            this.workerRegistrationPool = workerRegistrationPool;
            this.dataStoreManager = dataStoreManager;
        }

        /// <summary>
        /// Processes a message and dispatches any needed sub-tasks.
        /// </summary>
        /// <param name="workerRegistration">Registration for worker that sent the message.</param>
        /// <param name="rawCommand">Command string sent by worker.</param>
        public void ProcessMessage(WorkerRegistration workerRegistration, string rawCommand)
        {
            if (Regex.IsMatch(rawCommand, @"^HEARTBEAT$")) updateHeartbeat(workerRegistration);
            if (Regex.IsMatch(rawCommand, @"^VAR ")) dataStoreManager.ProcessCommand(rawCommand, workerRegistration);
        }

        private void updateHeartbeat(WorkerRegistration workerRegistration)
        {
            //sharedLogger.Debug($"Heartbeat update for {workerRegistration.Name}.");
            workerRegistration.LastHeartBeat = DateTime.Now;
        }
    }
}