using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Flintr_lib.Reports
{
    public class WorkerDetail
    {
        public string Name { get; set; }
        public int AssignedPort { get; set; }

        public DateTime LastReportedHeartbeat { get; set; }

        public WorkerDetail()
        {
        }

        public WorkerDetail(string name, int assignedPort, DateTime lastReportedHeartBeat)
        {
            Name = name;
            AssignedPort = assignedPort;
            LastReportedHeartbeat = lastReportedHeartBeat;
        }
    }
}