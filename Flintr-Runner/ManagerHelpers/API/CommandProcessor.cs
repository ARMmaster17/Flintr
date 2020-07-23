using Flintr_Runner.Communication;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.ManagerHelpers.API
{
    public class CommandProcessor
    {
        private JobDispatchManager jobDispatchManager;

        public CommandProcessor(JobDispatchManager jobDispatchManager)
        {
            this.jobDispatchManager = jobDispatchManager;
        }

        public void ExecuteJob(TCPClient client)
        {
            DispatchedJob dispatchedJob = jobDispatchManager.DispatchJob(client.RecieveObject<Job>());
            client.SendObject(dispatchedJob);
        }

        public void QueueJob(TCPClient client)
        {
            jobDispatchManager.QueueJob(client.RecieveObject<Job>());
        }
    }
}