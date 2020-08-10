using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.WorkerHelpers.Dispatch
{
    public enum TaskState
    {
        Initialized,
        Running,
        Finished,
        Errored
    }
}
