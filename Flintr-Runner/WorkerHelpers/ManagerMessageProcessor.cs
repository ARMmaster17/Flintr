using Flintr_lib.Communication;
using Flintr_lib.Jobs;
using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.WorkerHelpers
{
    public class ManagerMessageProcessor
    {
        private Logger.Logger logger;

        public ManagerMessageProcessor(RuntimeConfiguration runtimeConfiguration)
        {
            logger = runtimeConfiguration.GetLoggerInstance();
        }

        public void ProcessMessage(string rawCommand, TCPClient client)
        {
            JobStrategy solo = JobStrategy.RunOnOne;
            if (Regex.IsMatch(rawCommand, @"^EXECUTE\[\d+\] .+$")) processExecuteCommand(rawCommand, client);
        }

        private void processExecuteCommand(string rawCommand, TCPClient client)
        {
            string className = Regex.Replace(rawCommand, @"^EXECUTE\[\d+\] ", "");
            Type jobType = Type.GetType(className);
            var mi = typeof(TCPClient).GetMethod("ReceiveObject");
            var roRef = mi.MakeGenericMethod(jobType);
            //dynamic job = Convert.ChangeType(roRef.Invoke(client, null), jobType);
            Job job = client.ReceiveObject<Job>();
            job.Execute();
        }
    }
}