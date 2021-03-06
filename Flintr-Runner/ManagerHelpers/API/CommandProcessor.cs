﻿using Flintr_lib.Communication;
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
            Job job = client.ReceiveObject<Job>();
            DispatchedJob dispatchedJob = jobDispatchManager.DispatchJob(job);
            JobDetail jobDetail = new JobDetail(dispatchedJob.JobID, false);
            client.SendObject<JobDetail>(jobDetail);
        }

        public void QueueJob(TCPClient client, string rawCommand)
        {
            Job job = client.ReceiveObject<Job>();
            jobDispatchManager.QueueJob(job);
        }

        private Type getJobClassName(string className)
        {
            return Type.GetType(className);
        }
    }
}