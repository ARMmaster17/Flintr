using Flintr_Runner.Communication;
using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Flintr_Runner.ManagerHelpers
{
    public class WorkerRegistrationManager
    {
        private TCPServer tcpServer;
        private bool shouldListen;
        private Logger.Logger sharedLogger;
        private WorkerRegistrationPool registrationPool;
        private IPAddress managerBindIP;

        public WorkerRegistrationManager(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool)
        {
            managerBindIP = runtimeConfiguration.GetManagerBindAddress();
            tcpServer = new TCPServer(managerBindIP, runtimeConfiguration.GetManagerComPort());
            shouldListen = false;
            registrationPool = workerRegistrationPool;
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
        }

        public void ListenAsync()
        {
            shouldListen = true;
            while (shouldListen)
            {
                Socket newClient = tcpServer.WaitForNextConnection();
                string redirectInfo = getNewWorkerRegistrationInformation(tcpServer.GetNextMessage(newClient));
                tcpServer.SendMesage(newClient, redirectInfo);
            }
        }

        public void StopListening()
        {
            shouldListen = false;
        }

        private string getNewWorkerRegistrationInformation(string infoString)
        {
            WorkerRegistration newRegistration = registrationPool.RegisterNewWorker();
            sharedLogger.Msg($"New worker registration '{newRegistration.Name}' on port {newRegistration.Port}");
            return Convert.ToString(newRegistration.Port);
        }
    }
}