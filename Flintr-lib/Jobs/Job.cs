using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Flintr_lib.Jobs
{
    [XmlInclude(typeof(EchoJob))]
    [XmlRoot("Job"), XmlType("Job")]
    public abstract class Job
    {
        public JobStrategy strategy { get; set; }
        public string JobName { get; set; }
        public string RunnerName { get; set; }

        public Job()
        {
        }

        public Job(JobStrategy jobStrategy)
        {
        }

        public void Preload(string jobName, string runnerName)
        {
            JobName = jobName;
            RunnerName = runnerName;
        }

        public void WriteLine(string message)
        {
            Console.WriteLine($"[OUTPUT] {DateTime.Now} - [{RunnerName}/{JobName}]: {message}");
        }

        public abstract void Execute();
    }
}