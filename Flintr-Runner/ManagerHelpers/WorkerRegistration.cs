using Flintr_Runner.Communication;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Flintr_Runner.ManagerHelpers
{
    public class WorkerRegistration
    {
        public string Name;
        public TCPServer ClientConnection;
        public DateTime LastHeartBeat;
        public int Port;

        public WorkerRegistration(string name, TCPServer clientConnection, int port, DateTime lastHeartBeat)
        {
            Name = name;
            ClientConnection = clientConnection;
            LastHeartBeat = lastHeartBeat;
            Port = port;
        }
    }
}