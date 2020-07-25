using Flintr_lib.Jobs;
using Flintr_Runner.Configuration;
using Flintr_Runner.Exceptions;
using Flintr_Runner.ManagerHelpers;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flintr_Runner.Runners
{
    /// <summary>
    /// Manages all workers in a cluster and provides an API interface for applications with job requests.
    /// </summary>
    public class Manager : Runner
    {
        private WorkerRegistrationManager workerRegistrationManager;
        private WorkerRegistrationPool workerRegistrationPool;
        private WorkerMessageProcessor workerMessageProcessor;
        private JobDispatchManager jobDispatchManager;
        private TimeSpan workerHeartbeatTimeout;
        private ApiMessageProcessor apiMessageProcessor;

        /// <summary>
        /// Initialize Manager with default settings, and import from runtime configuration as necessary.
        /// </summary>
        /// <param name="runtimeConfiguration">Active runtime configuration settings profile.</param>
        public Manager(RuntimeConfiguration runtimeConfiguration) : base(runtimeConfiguration)
        {
            workerHeartbeatTimeout = new TimeSpan(0, 0, runtimeConfiguration.GetWorkerHeartbeatTimeout());
        }

        /// <summary>
        /// De-initializer that warns all sub-services of imminent shutdown.
        /// </summary>
        ~Manager()
        {
            workerRegistrationManager.StopListening();
        }

        /// <summary>
        /// Normal work cycle. Checks for messages from workers, API clients, and the dispatch center and responds appropriately.
        /// </summary>
        public override void runWork()
        {
            checkMessages();
            workerHealthCheck();
            jobDispatchManager.ProcessJobQueue();
            apiMessageProcessor.CheckMessages();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Sets up all requisite services and starts listening for requests from workers, API clients, and internal dispatches.
        /// </summary>
        /// <param name="runtimeConfiguration">Active runtime configuration settings profile.</param>
        public override void Setup(RuntimeConfiguration runtimeConfiguration)
        {
            // TODO: Some of this can be moved up to the class initializer.
            workerRegistrationPool = new WorkerRegistrationPool(runtimeConfiguration);
            jobDispatchManager = new JobDispatchManager(runtimeConfiguration, workerRegistrationPool);
            apiMessageProcessor = new ApiMessageProcessor(runtimeConfiguration, workerRegistrationPool, jobDispatchManager);
            workerRegistrationManager = new WorkerRegistrationManager(runtimeConfiguration, workerRegistrationPool);
            workerMessageProcessor = new WorkerMessageProcessor(runtimeConfiguration, workerRegistrationPool);
            Task.Run(() => workerRegistrationManager.ListenAsync());
            Task.Run(() => apiMessageProcessor.ListenAsync());
            SharedLogger.Msg("Manager", "API", $"Manager is listening for registrations at {runtimeConfiguration.GetManagerBindAddress().ToString()}:{runtimeConfiguration.GetManagerComPort()}");
        }

        /// <summary>
        /// Checks for messages from workers and processes any queued items.
        /// </summary>
        private void checkMessages()
        {
            List<WorkerRegistration> regPool = workerRegistrationPool.GetRegistrationPool();

            lock (regPool)
            {
                foreach (WorkerRegistration wr in regPool)
                {
                    try
                    {
                        if (wr.ClientServer == null)
                        {
                            // Do nothing, client is still in registration phase.
                        }
                        else if (wr.ClientServer.MessageIsAvailable())
                        {
                            workerMessageProcessor.ProcessMessage(wr, wr.ClientServer.Receive());
                        }
                    }
                    catch (IOException e)
                    {
                        SharedLogger.Error("Manager", "Worker Registration Service", "A network error occurred while receiving a network message from a worker");
                        SharedLogger.ErrorStackTrace("Manager", "Worker Registration Service", new Exception($"Error checking for messages from worker '{wr.Name}'.", e));
                    }
                }
            }
        }

        /// <summary>
        /// Checks all workers in the registration pools and writes to the output log if the time elapsed
        /// since the last heartbeat message is larger than the user-configured threshold.
        /// </summary>
        private void workerHealthCheck()
        {
            List<WorkerRegistration> regPool = workerRegistrationPool.GetRegistrationPool();

            lock (regPool)
            {
                foreach (WorkerRegistration wr in regPool)
                {
                    if (wr.IsDead(workerHeartbeatTimeout))
                    {
                        SharedLogger.Warning("Manager", "Health Check", $"Worker {wr.Name} is not responding.");
                    }
                }
            }
        }

        /// <summary>
        /// Sends a raw command to a worker. Currently not in use and may be removed in the future.
        /// </summary>
        /// <param name="workerName">Name of worker to send command to.</param>
        /// <param name="rawCommand">Raw command string to send.</param>
        private void sendCommandToWorker(string workerName, string rawCommand)
        {
            // TODO: Remove if no use is found for this method.
            WorkerRegistration worker = workerRegistrationPool.GetRegistrationPool().FirstOrDefault(o => o.Name == workerName);
            if (worker == null) throw new WorkerDoesNotExistException();
            worker.ClientServer.Send(rawCommand);
        }
    }
}