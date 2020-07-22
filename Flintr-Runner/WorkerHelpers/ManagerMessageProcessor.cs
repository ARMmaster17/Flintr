using Flintr_Runner.Configuration;
using Flintr_Runner.Jobs;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.WorkerHelpers
{
    public class ManagerMessageProcessor
    {
        private Logger.Logger logger;

        public ManagerMessageProcessor(RuntimeConfiguration runtimeConfiguration)
        {
            logger = runtimeConfiguration.GetLoggerInstance();
        }

        public void ProcessMessage(string rawCommand)
        {
            JobStrategy solo = JobStrategy.RunOnOne;
            if (EchoJob.ComLineMatch(rawCommand)) new EchoJob(logger, solo).Execute(rawCommand);
        }
    }
}