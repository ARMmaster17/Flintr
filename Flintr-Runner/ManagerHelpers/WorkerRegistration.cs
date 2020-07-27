using Flintr_lib.Communication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Flintr_Runner.ManagerHelpers
{
    /// <summary>
    /// Holds registration values for a worker within the cluster.
    /// </summary>
    public class WorkerRegistration
    {
        public string Name;
        public TCPClient ClientServer;
        public DateTime LastHeartBeat;
        public int Port;

        /// <summary>
        /// Default constructor to create a worker registration to keep track of the worker's unique name, assigned port, and connection
        /// object.
        /// </summary>
        /// <param name="name">Unique assigned name from pool.</param>
        /// <param name="clientServer">Object to represent the worker's private socket connection.</param>
        /// <param name="port">Assigned port for private communication.</param>
        /// <param name="lastHeartBeat">The timestamp of the last received heartbeat message.</param>
        public WorkerRegistration(string name, TCPClient clientServer, int port, DateTime lastHeartBeat)
        {
            Name = name;
            ClientServer = clientServer;
            LastHeartBeat = lastHeartBeat;
            Port = port;
        }

        /// <summary>
        /// Checks if a worker's last heartbeat message timespan is larger than the 'dead' threshold specified in the runtime configuration.
        /// </summary>
        /// <param name="deadWorkerThreshold">Amount of time that can pass before a worker is considered 'dead'.</param>
        /// <returns>If time elapsed since last heartbeat message is greater than the specified threshold.</returns>
        public bool IsDead(TimeSpan deadWorkerThreshold)
        {
            TimeSpan timeSinceLastHeartbeat = DateTime.Now.Subtract(LastHeartBeat);
            return timeSinceLastHeartbeat > deadWorkerThreshold;
        }
    }
}