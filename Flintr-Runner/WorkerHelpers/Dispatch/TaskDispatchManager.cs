using Flintr_lib.Jobs;
using Flintr_Runner.Configuration;
using Flintr_Runner.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Flintr_Runner.WorkerHelpers.Dispatch
{
    public class TaskDispatchManager
    {
        private string workerName;
        private List<TaskRegistration> taskStatePool;
        private Logger sharedLogger;

        public TaskDispatchManager(RuntimeConfiguration runtimeConfiguration, string assignedWorkerName)
        {
            workerName = assignedWorkerName;
            taskStatePool = new List<TaskRegistration>();
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
        }

        public void DispatchTask(string taskId, Job job)
        {
            TaskRegistration registration = new TaskRegistration(taskId, job);
            taskStatePool.Add(registration);

            try
            {
                job.Preload(taskId, workerName);
            }
            catch(Exception e)
            {
                sharedLogger.Error(workerName, $"Initialization of task of ID '{registration.TaskId}' failed with exception {e.GetType().Name}.");
                sharedLogger.ErrorStackTrace(workerName, e);
                registration.LogFatalError();
                return;
            }
            
            Task.Run(() => trackTask(registration));
        }

        private void trackTask(TaskRegistration registration)
        {
            registration.LogStartTime();
            try
            {
                registration.Job.Execute();
                registration.LogEndTime();
            }
            catch(Exception e)
            {
                sharedLogger.Error(workerName, $"Task of ID '{registration.TaskId}' failed with exception {e.GetType().Name}.");
                sharedLogger.ErrorStackTrace(workerName, e);
                registration.LogFatalError();
            }
        }
    }
}
