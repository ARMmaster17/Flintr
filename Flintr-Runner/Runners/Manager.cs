using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flintr_Runner.Runners
{
    public class Manager : Runner
    {
        private WorkerRegistrationManager workerRegistrationManager;
        private WorkerRegistrationPool workerRegistrationPool;

        public Manager(RuntimeConfiguration runtimeConfiguration) : base(runtimeConfiguration)
        {
        }

        public override void runWork()
        {
            List<WorkerRegistration> workerList = workerRegistrationPool.GetRegistrationPool();
            SharedLogger.Debug($"Worker pool currently has {workerList.Count} registrations.");
            foreach (WorkerRegistration wr in workerList)
            {
                SharedLogger.Debug($"{wr.Name} on port {wr.Port}");
            }
            Thread.Sleep(1000);
        }

        public override void Setup(RuntimeConfiguration runtimeConfiguration)
        {
            workerRegistrationPool = new WorkerRegistrationPool(runtimeConfiguration);
            workerRegistrationManager = new WorkerRegistrationManager(runtimeConfiguration, workerRegistrationPool);
            Task.Run(() => workerRegistrationManager.ListenAsync());
            SharedLogger.Msg($"Manager is listening for registrations at {runtimeConfiguration.GetManagerBindAddress().ToString()}:{runtimeConfiguration.GetManagerComPort()}");
        }
    }
}