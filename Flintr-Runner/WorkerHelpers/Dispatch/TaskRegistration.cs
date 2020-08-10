using Flintr_lib.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.WorkerHelpers.Dispatch
{
    public class TaskRegistration
    {
        public string TaskId
        {
            get;
            private set;
        }

        public Job Job
        {
            get;
            private set;
        }

        public DateTime StartTime
        {
            get;
            private set;
        }

        public DateTime EndTime
        {
            get;
            private set;
        }

        public TaskState State
        {
            get;
            private set;
        }

        /// <summary>
        /// Default constructor. Creates a tracking object to report the status of a running job once it is externally triggered.
        /// </summary>
        /// <param name="taskId">Assigned globally-unique ID for this task.</param>
        /// <param name="job">Instance of a Job that contains all neccesary data to issue an Execute() command.</param>
        public TaskRegistration(string taskId, Job job)
        {
            TaskId = taskId;
            Job = job;
            State = TaskState.Initialized;
        }

        /// <summary>
        /// Enters the current time as the start time for a running task. Should be triggered right before Execute().
        /// </summary>
        public void LogStartTime()
        {
            StartTime = DateTime.Now;
            State = TaskState.Running;
        }

        /// <summary>
        /// Enters the current time as the end time for a running task. Should be triggered right after Execute().
        /// </summary>
        public void LogEndTime()
        {
            EndTime = DateTime.Now;
            State = TaskState.Finished;
        }

        /// <summary>
        /// Enters the current time as the end time for a running task and marks the task as errored.
        /// </summary>
        public void LogFatalError()
        {
            LogEndTime();
            State = TaskState.Errored;
        }
    }
}
