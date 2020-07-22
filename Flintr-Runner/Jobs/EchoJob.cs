using Flintr_Runner.ManagerHelpers.Dispatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.Jobs
{
    public class EchoJob : Job
    {
        private string echoLine;
        private Logger.Logger logger;

        public EchoJob(Logger.Logger sharedLogger, JobStrategy jobStrategy) : base(jobStrategy)
        {
            logger = sharedLogger;
        }

        public EchoJob(Logger.Logger sharedLogger, JobStrategy jobStrategy, string line) : base(jobStrategy)
        {
            logger = sharedLogger;
            echoLine = line;
        }

        public override void Execute(string rawLine)
        {
            base.Execute(rawLine);
            logger.Msg(echoLine);
        }

        public override string GetComLine(int taskID, string args)
        {
            return base.GetComLine(taskID, $"ECHO {echoLine}");
        }

        public override void LoadFromComLine(string rawLine)
        {
            echoLine = Regex.Replace(rawLine, @"^DISPATCHTASK\[\d+\] ECHO ", "");
        }

        public static bool ComLineMatch(string rawLine)
        {
            return Regex.IsMatch(rawLine, @"^DISPATCHTASK\[\d+\] ECHO .+");
        }
    }
}