using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Net;

namespace Flintr_Runner.ManagerHelpers
{
    /// <summary>
    /// Handles a list of all registered workers and their current status, name, and connection info.
    /// </summary>
    public class WorkerRegistrationPool
    {
        private List<WorkerRegistration> registrationPool;
        private IPAddress hostAddress;
        private int basePort;
        private RuntimeConfiguration runtimeConfiguration;

        /// <summary>
        /// Initializes the pool with default settings and binds to the active
        /// Logger output.
        /// </summary>
        /// <param name="runtimeConfiguration">Active configuration settings for the current session.</param>
        public WorkerRegistrationPool(RuntimeConfiguration runtimeConfiguration)
        {
            this.runtimeConfiguration = runtimeConfiguration;
            registrationPool = new List<WorkerRegistration>();
            hostAddress = runtimeConfiguration.GetManagerBindAddress();
            basePort = runtimeConfiguration.GetManagerComPort() + 1;
        }

        /// <summary>
        /// Registers a new worker by assigning a GUID and sets all health check values to their initial values.
        /// Does not attempt a network connection initialization or handshake (this is handled by WorkerRegistrationManager).
        /// </summary>
        /// <returns>Registration info for the next worker to connect.</returns>
        public WorkerRegistration RegisterNewWorker()
        {
            int port = basePort + registrationPool.Count;
            DateTime firstHeartBeat = DateTime.Now;
            string name = "worker-" + (registrationPool.Count + 1);
            WorkerRegistration registration = new WorkerRegistration(name, null, port, firstHeartBeat);
            AddToPool(registration);
            return registration;
        }

        /// <summary>
        /// Gets a reference to the active registration pool. The responsibility remains with the method caller to properly
        /// lock the pool before iterating or performing add/delete operations on registrations.
        /// </summary>
        /// <returns>Reference to active worker pool.</returns>
        public List<WorkerRegistration> GetRegistrationPool()
        {
            return registrationPool;
        }

        /// <summary>
        /// Gets a list of all worker registrations of all workers that are not currently in a 'dead' state (time elapsed
        /// since the last heartbeat message is over the 'dead' threshold).
        /// </summary>
        /// <returns>List of registrations of all non-dead workers.</returns>
        public List<WorkerRegistration> GetNonDeadWorkerPool()
        {
            List<WorkerRegistration> result;
            TimeSpan timeoutSetting = new TimeSpan(0, 0, runtimeConfiguration.GetWorkerHeartbeatTimeout());
            lock (registrationPool)
            {
                result = GetRegistrationPool().FindAll(o => !o.IsDead(timeoutSetting));
            }
            return result;
        }

        /// <summary>
        /// Adds a new worker registration to the pool.
        /// </summary>
        /// <param name="workerRegistration">Registration details of new worker.</param>
        public void AddToPool(WorkerRegistration workerRegistration)
        {
            lock (registrationPool)
            {
                registrationPool.Add(workerRegistration);
            }
            runtimeConfiguration.GetLoggerInstance().Msg("Manager", "Worker Registration Pool", $"Worker with name '{workerRegistration.Name}' was added to the pool.");
        }
    }
}