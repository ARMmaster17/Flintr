using Flintr_lib.Reports;
using Flintr_Runner.Communication;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Flintr_Runner.ManagerHelpers.API
{
    public class ReportProcessor
    {
        private WorkerRegistrationPool workerRegistrationPool;
        private JobDispatchManager jobDispatchManager;

        public ReportProcessor(WorkerRegistrationPool workerRegistrationPool, JobDispatchManager jobDispatchManager)
        {
            this.workerRegistrationPool = workerRegistrationPool;
            this.jobDispatchManager = jobDispatchManager;
        }

        public void ProcessReportRequest(TCPClient client, string rawRequest)
        {
            string specificRequest = Regex.Replace(rawRequest, @"^REPORT ", "");
            if (Regex.IsMatch(specificRequest, @"^LISTWORKER .+$")) listWorker(client, specificRequest);
        }

        public static bool IsReportRequest(string rawRequest)
        {
            return Regex.IsMatch(rawRequest, @"^REPORT .+");
        }

        private void listWorker(TCPClient client, string rawRequest)
        {
            string specificRequest = Regex.Replace(rawRequest, @"^LISTWORKER ", "");
            string workerIndex = specificRequest;
            WorkerRegistration wr = workerRegistrationPool.GetRegistrationPool().Find(o => o.Name == workerIndex);
            client.SendObject(new WorkerDetail(wr.Name, wr.Port, wr.LastHeartBeat));
        }
    }
}