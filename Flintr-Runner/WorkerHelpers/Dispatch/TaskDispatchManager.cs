using Flintr_lib.Jobs;
using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Flintr_Runner.WorkerHelpers.Dispatch
{
    public class TaskDispatchManager
    {
        private string workerName;

        public TaskDispatchManager(RuntimeConfiguration runtimeConfiguration, string assignedWorkerName)
        {
            workerName = assignedWorkerName;
        }

        public void DispatchTask(string taskId, Job job)
        {
            job.Preload(taskId, workerName);
            Task.Run(() => trackTask(taskId));
        }

        private void trackTask(string taskId)
        {

        }
    }
}
