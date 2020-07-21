using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.Runners
{
    public abstract class Runner
    {
        protected Logger.Logger SharedLogger;

        private bool isRunning;

        public Runner(RuntimeConfiguration runtimeConfiguration)
        {
            SharedLogger = runtimeConfiguration.GetLoggerInstance();
            isRunning = true;
        }

        public virtual void Setup(RuntimeConfiguration runtimeConfiguration)
        {
        }

        public void RunAsync()
        {
            while (isRunning)
            {
                runWork();
            }
        }

        public void Kill()
        {
            isRunning = false;
        }

        public abstract void runWork();
    }
}