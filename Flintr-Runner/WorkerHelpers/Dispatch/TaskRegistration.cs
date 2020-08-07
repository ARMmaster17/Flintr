using Flintr_lib.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.WorkerHelpers.Dispatch
{
    public class TaskRegistration
    {
        string taskId;
        Job job;
        DateTime startTime;
        DateTime endTime;
    }
}
