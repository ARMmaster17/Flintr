using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_lib.Reports
{
    [Serializable()]
    public class JobDetail
    {
        public int JobID { get; protected set; }
        public bool IsComplete { get; protected set; }

        public JobDetail(int jobId, bool isComplete)
        {
            JobID = jobId;
            IsComplete = isComplete;
        }
    }
}