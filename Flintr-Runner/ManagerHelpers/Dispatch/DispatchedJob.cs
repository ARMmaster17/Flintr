using Flintr_lib.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.ManagerHelpers.Dispatch
{
    public class DispatchedJob
    {
        private List<DispatchedTask> subTasks;
        private Job baseJob;
        public int JobID { get; protected set; }

        public DispatchedJob(List<DispatchedTask> dispatchedTasks, Job job, int jobId)
        {
            baseJob = job;
            subTasks = dispatchedTasks;
            JobID = jobId;
        }

        public bool IsComplete()
        {
            foreach (DispatchedTask task in subTasks)
            {
                if (!task.TaskComplete) return false;
            }
            return true;
        }
    }
}