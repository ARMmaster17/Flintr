using Flintr_lib.Communication;
using Flintr_lib.Jobs;
using Flintr_lib.Reports;
using System;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Flintr_lib
{
    public sealed class FlintrInstance
    {
        private TCPClient managerConnection;

        public FlintrInstance()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            int port = 3999;
            IPEndPoint flintrManager = new IPEndPoint(address, port);
            Connect(flintrManager);
        }

        public FlintrInstance(IPAddress flintrManagerAddress, int flintrExecPort)
        {
            IPEndPoint flintrManager = new IPEndPoint(flintrManagerAddress, flintrExecPort);
            Connect(flintrManager);
        }

        public FlintrInstance(IPEndPoint flintrManager)
        {
            Connect(flintrManager);
        }

        public void Connect(IPEndPoint flintrManager)
        {
            managerConnection = new TCPClient(flintrManager);
        }

        public WorkerDetail GetWorkerDetails(string workerName)
        {
            managerConnection.Send($"REPORT LISTWORKER {workerName}");
            WorkerDetail workerDetail = managerConnection.ReceiveObject<WorkerDetail>();
            return workerDetail;
        }

        public JobDetail ExecuteRawJob(Job job)
        {
            managerConnection.Send($"EXECUTE {job.GetType().AssemblyQualifiedName}");
            managerConnection.SendObject(job);
            return managerConnection.ReceiveObject<JobDetail>();
        }

        public void QueueRawJob(Job job)
        {
            managerConnection.Send($"QueueJob {job.GetType().AssemblyQualifiedName}");
            managerConnection.SendObject(job);
        }
    }
}