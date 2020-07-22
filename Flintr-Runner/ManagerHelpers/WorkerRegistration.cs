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
        public TCPClient ClientServer;
        public DateTime LastHeartBeat;
        public int Port;

        public WorkerRegistration(string name, TCPClient clientServer, int port, DateTime lastHeartBeat)
        {
            Name = name;
            ClientServer = clientServer;
            LastHeartBeat = lastHeartBeat;
            Port = port;
        }

        public bool IsDead()
        {
            TimeSpan timeSinceLastHeartbeat = DateTime.Now.Subtract(LastHeartBeat);
            return timeSinceLastHeartbeat > new TimeSpan(0, 0, 5);
        }
    }
}