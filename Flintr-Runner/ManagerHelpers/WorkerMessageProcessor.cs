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

        /// <summary>
        /// Default constructor with default settings.
        /// </summary>
        /// <param name="runtimeConfiguration">Active runtime configuration settings for current session.</param>
        /// <param name="workerRegistrationPool">Active pool of workers to pull information from.</param>
        public WorkerMessageProcessor(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool)
        {
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            this.workerRegistrationPool = workerRegistrationPool;
        }

        /// <summary>
        /// Processes a message and dispatches any needed sub-tasks.
        /// </summary>
        /// <param name="workerRegistration"></param>
        /// <param name="rawCommand"></param>
        public void ProcessMessage(WorkerRegistration workerRegistration, string rawCommand)
        {
            if (Regex.IsMatch(rawCommand, @"^HEARTBEAT$")) updateHeartbeat(workerRegistration);
        }

        private void updateHeartbeat(WorkerRegistration workerRegistration)
        {
            //sharedLogger.Debug($"Heartbeat update for {workerRegistration.Name}.");
            workerRegistration.LastHeartBeat = DateTime.Now;
        }
    }
}