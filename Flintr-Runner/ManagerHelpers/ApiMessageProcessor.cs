using Flintr_lib.Communication;
using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers.API;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.ManagerHelpers
{
    public class ApiMessageProcessor
    {
        private TCPServer apiServer;
        private WorkerRegistrationPool workerPool;
        private JobDispatchManager jobDispatchManager;
        private List<TCPClient> activeClients;
        private Logger.Logger sharedLogger;
        private ReportProcessor reportProcessor;
        private CommandProcessor commandProcessor;

        public ApiMessageProcessor(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool, JobDispatchManager jobDispatchManager)
        {
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            apiServer = new TCPServer(runtimeConfiguration.GetManagerBindAddress(), runtimeConfiguration.GetManagerExecPort());
            workerPool = workerRegistrationPool;
            this.jobDispatchManager = jobDispatchManager;
            reportProcessor = new ReportProcessor(workerRegistrationPool, jobDispatchManager);
            activeClients = new List<TCPClient>();
            commandProcessor = new CommandProcessor(jobDispatchManager);
        }

        public void ListenAsync()
        {
            while (true)
            {
                activeClients.Add(apiServer.WaitForNextConnection());
            }
        }

        public void CheckMessages()
        {
            foreach (TCPClient client in activeClients)
            {
                if (client.MessageIsAvailable())
                {
                    processMessage(client, client.Receive());
                }
            }
        }

        private void processMessage(TCPClient client, string rawCommand)
        {
            if (ReportProcessor.IsReportRequest(rawCommand)) reportProcessor.ProcessReportRequest(client, rawCommand);
            else if (Regex.IsMatch(rawCommand, @"^EXECUTE$")) commandProcessor.ExecuteJob(client, rawCommand);
            else if (Regex.IsMatch(rawCommand, @"^QUEUEJOB$")) commandProcessor.QueueJob(client, rawCommand);
        }
    }
}