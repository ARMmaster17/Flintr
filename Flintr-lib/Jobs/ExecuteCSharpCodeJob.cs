using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_lib.Jobs
{
    public class ExecuteCSharpCodeJob : Job
    {
        private string completeCode;

        public ExecuteCSharpCodeJob()
        {
        }

        public ExecuteCSharpCodeJob(JobStrategy jobStrategy, string completeCodeFileContents) : base(jobStrategy)
        {
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}