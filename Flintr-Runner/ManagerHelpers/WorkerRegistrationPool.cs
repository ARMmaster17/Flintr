using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Flintr_Runner.ManagerHelpers
{
    public class WorkerRegistrationPool
    {
        private List<WorkerRegistration> registrationPool;
        private IPAddress hostAddress;
        private int basePort;

        private bool poolLocked;

        private RuntimeConfiguration runtimeConfiguration;

        public WorkerRegistrationPool(RuntimeConfiguration runtimeConfiguration)
        {
            this.runtimeConfiguration = runtimeConfiguration;
            registrationPool = new List<WorkerRegistration>();
            hostAddress = runtimeConfiguration.GetManagerBindAddress();
            basePort = runtimeConfiguration.GetManagerComPort() + 1;
            poolLocked = false;
        }

        public WorkerRegistration RegisterNewWorker()
        {
            int port = basePort + registrationPool.Count;
            DateTime firstHeartBeat = DateTime.Now;
            string name = "worker-" + (registrationPool.Count + 1);
            WorkerRegistration registration = new WorkerRegistration(name, null, port, firstHeartBeat);
            AddToPool(registration);
            return registration;
        }

        public void LockRegistrationPool()
        {
            while (poolLocked) { Thread.Sleep(500); }
            poolLocked = true;
        }

        public List<WorkerRegistration> GetRegistrationPool()
        {
            return registrationPool;
        }

        public List<WorkerRegistration> GetNonDeadWorkerPool()
        {
            return GetRegistrationPool().FindAll(o => !o.IsDead());
        }

        public void AddToPool(WorkerRegistration workerRegistration)
        {
            while (poolLocked) { Thread.Sleep(500); }
            registrationPool.Add(workerRegistration);
        }

        public void UnlockRegistrationPool()
        {
            poolLocked = false;
        }
    }
}