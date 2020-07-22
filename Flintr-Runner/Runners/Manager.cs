using Flintr_Runner.Communication;
using Flintr_Runner.Configuration;
using Flintr_Runner.Exceptions;
using Flintr_Runner.Jobs;
using Flintr_Runner.ManagerHelpers;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private WorkerMessageProcessor workerMessageProcessor;
        private JobDispatchManager jobDispatchManager;
        private TimeSpan workerHeartbeatTimeout;

        public Manager(RuntimeConfiguration runtimeConfiguration) : base(runtimeConfiguration)
        {
            workerHeartbeatTimeout = new TimeSpan(0, 0, runtimeConfiguration.GetWorkerHeartbeatTimeout());
        }

        public override void runWork()
        {
            checkMessages();
            workerHealthCheck();
            ///////////////////////////////
            EchoJob echoJob = new EchoJob(SharedLogger, JobStrategy.RunOnAll, "TEST BROADCAST");
            jobDispatchManager.DispatchJob(echoJob);
            ///////////////////////////////
            Thread.Sleep(1000);
        }

        public override void Setup(RuntimeConfiguration runtimeConfiguration)
        {
            workerRegistrationPool = new WorkerRegistrationPool(runtimeConfiguration);
            jobDispatchManager = new JobDispatchManager(runtimeConfiguration, workerRegistrationPool);
            workerRegistrationManager = new WorkerRegistrationManager(runtimeConfiguration, workerRegistrationPool);
            workerMessageProcessor = new WorkerMessageProcessor(runtimeConfiguration, workerRegistrationPool);
            Task.Run(() => workerRegistrationManager.ListenAsync());
            SharedLogger.Msg($"Manager is listening for registrations at {runtimeConfiguration.GetManagerBindAddress().ToString()}:{runtimeConfiguration.GetManagerComPort()}");
        }

        private void checkMessages()
        {
            workerRegistrationPool.LockRegistrationPool();
            foreach (WorkerRegistration wr in workerRegistrationPool.GetRegistrationPool())
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
            workerRegistrationPool.UnlockRegistrationPool();
        }

        private void workerHealthCheck()
        {
            workerRegistrationPool.LockRegistrationPool();
            foreach (WorkerRegistration wr in workerRegistrationPool.GetRegistrationPool())
            {
                TimeSpan timeSinceLastHeartbeat = DateTime.Now.Subtract(wr.LastHeartBeat);
                if (timeSinceLastHeartbeat > new TimeSpan(0, 0, 5))
                {
                    SharedLogger.Warning($"Worker {wr.Name} has been dead for over {Math.Round(timeSinceLastHeartbeat.TotalSeconds, 0)} seconds!");
                }
            }
            workerRegistrationPool.UnlockRegistrationPool();
        }

        private void sendCommandToWorker(string workerName, string rawCommand)
        {
            WorkerRegistration worker = workerRegistrationPool.GetRegistrationPool().FirstOrDefault(o => o.Name == workerName);
            if (worker == null) throw new WorkerDoesNotExistException();
            worker.ClientServer.Send(rawCommand);
        }
    }
}