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
        private RuntimeConfiguration runtimeConfiguration;

        public WorkerRegistrationManager(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool)
        {
            managerBindIP = runtimeConfiguration.GetManagerBindAddress();
            tcpServer = new TCPServer(managerBindIP, runtimeConfiguration.GetManagerComPort(), runtimeConfiguration);
            shouldListen = false;
            registrationPool = workerRegistrationPool;
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            this.runtimeConfiguration = runtimeConfiguration;
        }

        public void ListenAsync()
        {
            shouldListen = true;
            while (shouldListen)
            {
                TCPClient newClient = tcpServer.WaitForNextConnection();
                WorkerRegistration registration = getNewWorkerRegistrationInformation(newClient.Receive());
                newClient.Send(registration.Port.ToString());
                sharedLogger.Debug($"Transferring {registration.Name} to new connection...");
                TCPServer newConnection = new TCPServer(managerBindIP, registration.Port, runtimeConfiguration);
                registration.ClientServer = newConnection.WaitForNextConnection();
                sharedLogger.Debug($"Connection successful on new channel.");
            }
        }

        public void StopListening()
        {
            shouldListen = false;
        }

        private WorkerRegistration getNewWorkerRegistrationInformation(string infoString)
        {
            WorkerRegistration newRegistration = registrationPool.RegisterNewWorker();
            sharedLogger.Msg($"New worker registration '{newRegistration.Name}' on port {newRegistration.Port}");
            return newRegistration;
        }
    }
}