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
            BinaryFormatter deserializer = new BinaryFormatter();
            WorkerDetail workerDetail = (WorkerDetail)deserializer.Deserialize(managerConnection.GetNetworkStreamReader().BaseStream);
            return workerDetail;
        }

        public void ExecuteRawJobOne(string rawCommand)
        {
            managerConnection.Send($"EXECUTE ONE {rawCommand}");
        }

        public void ExecuteRawJobALL(string rawCommand)
        {
            managerConnection.Send($"EXECUTE ALL {rawCommand}");
        }
    }
}