using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.ManagerHelpers.Dispatch
{
    public abstract class Job
    {
        public JobStrategy strategy { get; protected set; }

        public Job(JobStrategy jobStrategy)
        {
        }

        public virtual void Execute(string rawLine)
        {
            LoadFromComLine(rawLine);
        }

        public virtual string GetComLine(int taskID, string args)
        {
            return $"DISPATCHTASK[{taskID}] " + args;
        }

        public abstract void LoadFromComLine(string rawLine);
    }
}