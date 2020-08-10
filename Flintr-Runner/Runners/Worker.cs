using Flintr_lib.Communication;
using Flintr_Runner.Configuration;
using Flintr_Runner.WorkerHelpers;
using Flintr_Runner.WorkerHelpers.Dispatch;
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
        private TaskDispatchManager taskDispatchManager;

        /// <summary>
        /// Default constructor. Configures runner with specified runtime configuration.
        /// </summary>
        /// <param name="runtimeConfiguration">Initialized configuration to pull runtime settings from.</param>
        public Worker(RuntimeConfiguration runtimeConfiguration) : base(runtimeConfiguration)
        {
            workerName = "Unassigned";
        }

        /// <summary>
        /// Looped function that checks for messages from the manager and performs neccesary system checks.
        /// </summary>
        public override void runWork()
        {
            managerConnection.SendObject<string>("HEARTBEAT");
            checkForMessages();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Performs setup functions for runner including connection to a manager instance.
        /// </summary>
        /// <param name="runtimeConfiguration">Initialized configuration to pull runtime settings from.</param>
        public override void Setup(RuntimeConfiguration runtimeConfiguration)
        {
            base.Setup(runtimeConfiguration);
            registerWorker(runtimeConfiguration.GetManagerBindAddress(), runtimeConfiguration.GetManagerComPort());
            taskDispatchManager = new TaskDispatchManager(runtimeConfiguration, workerName);
            managerMessageProcessor = new ManagerMessageProcessor(runtimeConfiguration, workerName, taskDispatchManager);
        }

        /// <summary>
        /// Handles the registration of the worker runner to a manager instance to recieve commands from.
        /// </summary>
        /// <param name="managerAddress">IP Address of manager to connect to.</param>
        /// <param name="registrationPort">API port to connect to (default 3999).</param>
        private void registerWorker(IPAddress managerAddress, int registrationPort)
        {
            getNewPort(managerAddress, registrationPort);
            managerConnection = new TCPClient(managerAddress, assignedPort);
            managerConnection.SendObject<string>("TRANSFER");
        }

        /// <summary>
        /// Sub-task of registration process that handles the retrieval and decoding of port assignment for communication with the manager instance.
        /// </summary>
        /// <param name="managerAddress">IP Address of manager to connect to.</param>
        /// <param name="registrationPort">API port to connect to (default 3999).</param>
        private void getNewPort(IPAddress managerAddress, int registrationPort)
        {
            TCPClient registrationConnection = new TCPClient(managerAddress, registrationPort);
            SharedLogger.Debug(workerName, "Registration Service", $"Registration sent to {managerAddress}:{registrationPort}.");
            registrationConnection.SendObject<string>("REGISTER");
            string[] registrationInfo = registrationConnection.ReceiveObject<string>().Split('|');
            workerName = registrationInfo[1];
            SharedLogger.Msg(workerName, "Registration Service", $"Registered to manager server at {managerAddress.ToString()} with port assignment {registrationInfo[0]}.");
            assignedPort = Convert.ToInt32(registrationInfo[0]);
        }

        /// <summary>
        /// Checks the active network connections for new messages and launching parsers to interpret the messages.
        /// </summary>
        private void checkForMessages()
        {
            if (managerConnection.MessageIsAvailable())
            {
                managerMessageProcessor.ProcessMessage(managerConnection.ReceiveObject<string>(), managerConnection);
            }
        }
    }
}