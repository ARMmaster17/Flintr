using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Flintr_lib.Reports
{
    [Serializable()]
    public class WorkerDetail
    {
        public string Name { get; private set; }
        public int AssignedPort { get; private set; }

        public DateTime LastReportedHeartbeat { get; private set; }

        public WorkerDetail(string name, int assignedPort, DateTime lastReportedHeartBeat)
        {
            Name = name;
            AssignedPort = assignedPort;
            LastReportedHeartbeat = lastReportedHeartBeat;
        }
    }
}