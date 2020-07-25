using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Flintr_lib.Jobs
{
    [XmlInclude(typeof(EchoJob))]
    [XmlRoot("Job"), XmlType("Job")]
    public abstract class Job
    {
        public JobStrategy strategy { get; set; }

        public Job()
        {
        }

        public Job(JobStrategy jobStrategy)
        {
        }

        public abstract void Execute();
    }
}