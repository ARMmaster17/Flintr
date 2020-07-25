using Flintr_lib.Communication;
using Flintr_lib.Jobs;
using Flintr_lib.Reports;
using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.ManagerHelpers.API
{
    public class CommandProcessor
    {
        private JobDispatchManager jobDispatchManager;

        public CommandProcessor(JobDispatchManager jobDispatchManager)
        {
            this.jobDispatchManager = jobDispatchManager;
        }

        public void ExecuteJob(TCPClient client, string rawCommand)
        {
            string className = Regex.Replace(rawCommand, @"^EXECUTE ", "");
            Type jobType = getJobClassName(className);
            var mi = typeof(TCPClient).GetMethod("RecieveObject");
            var roRef = mi.MakeGenericMethod(jobType);
            dynamic job = Convert.ChangeType(roRef.Invoke(client, null), jobType);
            DispatchedJob dispatchedJob = jobDispatchManager.DispatchJob(job);
            JobDetail jobDetail = new JobDetail(dispatchedJob.JobID, false);
            client.SendObject<JobDetail>(jobDetail);
        }

        public void QueueJob(TCPClient client, string rawCommand)
        {
            string className = Regex.Replace(rawCommand, @"^QUEUEJOB ", "");
            Type jobType = getJobClassName(className);
            var mi = typeof(TCPClient).GetMethod("RecieveObject");
            var roRef = mi.MakeGenericMethod(jobType);
            dynamic job = Convert.ChangeType(roRef.Invoke(client, null), jobType);
            jobDispatchManager.QueueJob(job);
        }

        private Type getJobClassName(string className)
        {
            return Type.GetType(className);
        }
    }
}