using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.ManagerHelpers
{
    public class WorkerMessageProcessor
    {
        private Logger.Logger sharedLogger;
        private WorkerRegistrationPool workerRegistrationPool;

        public WorkerMessageProcessor(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool)
        {
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            this.workerRegistrationPool = workerRegistrationPool;
        }

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