using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.ManagerHelpers.Dispatch
{
    public class DispatchedJob
    {
        private List<DispatchedTask> subTasks;
        private Job baseJob;

        public DispatchedJob(List<DispatchedTask> dispatchedTasks, Job job)
        {
            baseJob = job;
            subTasks = dispatchedTasks;
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