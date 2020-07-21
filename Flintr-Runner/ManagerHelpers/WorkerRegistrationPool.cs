using Flintr_Runner.Communication;
using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Flintr_Runner.ManagerHelpers
{
    public class WorkerRegistrationPool
    {
        private List<WorkerRegistration> registrationPool;
        private IPAddress hostAddress;
        private int basePort;

        public WorkerRegistrationPool(RuntimeConfiguration runtimeConfiguration)
        {
            registrationPool = new List<WorkerRegistration>();
            hostAddress = runtimeConfiguration.GetManagerBindAddress();
            basePort = runtimeConfiguration.GetManagerComPort() + 1;
        }

        public WorkerRegistration RegisterNewWorker()
        {
            int port = basePort + registrationPool.Count;
            TCPServer clientManager = new TCPServer(hostAddress, port);
            DateTime firstHeartBeat = DateTime.Now;
            string name = "worker-" + (registrationPool.Count + 1);
            WorkerRegistration registration = new WorkerRegistration(name, clientManager, port, firstHeartBeat);
            registrationPool.Add(registration);
            return registration;
        }

        public List<WorkerRegistration> GetRegistrationPool()
        {
            return registrationPool;
        }
    }
}