using Flintr_Runner.Communication;
using Flintr_Runner.Configuration;
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

        public Worker(RuntimeConfiguration runtimeConfiguration) : base(runtimeConfiguration)
        {
        }

        public override void runWork()
        {
            //SharedLogger.Debug("Worker doing work");
        }

        public override void Setup(RuntimeConfiguration runtimeConfiguration)
        {
            base.Setup(runtimeConfiguration);
            registerWorker(runtimeConfiguration.GetManagerBindAddress(), runtimeConfiguration.GetManagerComPort());
        }

        private void registerWorker(IPAddress managerAddress, int registrationPort)
        {
            TCPClient registrationConnection = new TCPClient(managerAddress, registrationPort);
            registrationConnection.Send("REGISTER");
            while (!registrationConnection.MessageIsAvailable())
            {
                Thread.Sleep(1000);
            }
            string registrationInfo = registrationConnection.Recieve();
            SharedLogger.Msg($"Registered to manager server at {managerAddress.ToString()}");
            SharedLogger.Debug(registrationInfo);
            int newPort = Convert.ToInt32(registrationInfo);
            managerConnection = new TCPClient(managerAddress, newPort);
        }
    }
}