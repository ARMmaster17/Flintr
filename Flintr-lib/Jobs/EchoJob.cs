using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_lib.Jobs
{
    public class EchoJob : Job
    {
        public string echoLine { get; set; }

        public EchoJob()
        {
        }

        public EchoJob(JobStrategy jobStrategy, string echoLine) : base(jobStrategy)
        {
            this.echoLine = echoLine;
        }

        public override void Execute()
        {
            WriteLine(echoLine);
        }
    }
}