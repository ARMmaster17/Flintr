using Flintr_Runner.Communication;
using Flintr_Runner.Configuration;
using Flintr_Runner.WorkerHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Flintr_Runner.Runners
{
    public class Worker : Runner
    {
        private TCPClient managerConnection;
        private string workerName;
        private int assignedPort;
        private ManagerMessageProcessor managerMessageProcessor;

        public Worker(RuntimeConfiguration runtimeConfiguration) : base(runtimeConfiguration)
        {
            managerMessageProcessor = new ManagerMessageProcessor(runtimeConfiguration);
        }

        public override void runWork()
        {
            managerConnection.Send("HEARTBEAT");
            checkForMessages();
            Thread.Sleep(1000);
        }

        public override void Setup(RuntimeConfiguration runtimeConfiguration)
        {
            base.Setup(runtimeConfiguration);
            registerWorker(runtimeConfiguration.GetManagerBindAddress(), runtimeConfiguration.GetManagerComPort());
        }

        private void registerWorker(IPAddress managerAddress, int registrationPort)
        {
            getNewPort(managerAddress, registrationPort);
            managerConnection = new TCPClient(managerAddress, assignedPort);
            managerConnection.Send("TRANSFER");
        }

        private void getNewPort(IPAddress managerAddress, int registrationPort)
        {
            TCPClient registrationConnection = new TCPClient(managerAddress, registrationPort);
            SharedLogger.Debug($"Registration sent to {managerAddress}:{registrationPort}.");
            registrationConnection.Send("REGISTER");
            //while (!registrationConnection.MessageIsAvailable())
            //{
            //    Thread.Sleep(500);
            //}
            string registrationInfo = null;
            registrationInfo = registrationConnection.Receive();
            SharedLogger.Msg($"Registered to manager server at {managerAddress.ToString()}");
            SharedLogger.Debug(registrationInfo);
            assignedPort = Convert.ToInt32(registrationInfo);
        }

        private void checkForMessages()
        {
            if (managerConnection.MessageIsAvailable())
            {
                managerMessageProcessor.ProcessMessage(managerConnection.Receive());
            }
        }
    }
}