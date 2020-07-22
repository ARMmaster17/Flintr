using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.ManagerHelpers.Dispatch
{
    public class DispatchedTask
    {
        public string AssignedWorker { get; private set; }
        public int TaskID { get; private set; }

        public bool TaskComplete;

        public DispatchedTask(int taskID, string assignedWorker)
        {
            TaskID = taskID;
            AssignedWorker = assignedWorker;
            TaskComplete = false;
        }
    }
}