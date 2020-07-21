using Flintr_Runner.Configuration;
using Flintr_Runner.Runners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Flintr_Runner.Pool
{
    public class WorkerPool
    {
        private List<Worker> pool;

        public WorkerPool(Configuration.RuntimeConfiguration runtimeConfiguration)
        {
            pool = new List<Worker>();
            for (int i = 1; i <= runtimeConfiguration.GetWorkerThreadCount(); i++)
            {
                pool.Add(new Worker(runtimeConfiguration));
            }
        }

        public void SetupAllWorkers(RuntimeConfiguration runtimeConfiguration)
        {
            foreach (Worker worker in pool)
            {
                worker.Setup(runtimeConfiguration);
            }
        }

        public void RunAllWorkersAsync()
        {
            foreach (Worker worker in pool)
            {
                Task.Run(() => worker.RunAsync());
            }
        }

        public void KillAllWorkers()
        {
            foreach (Worker worker in pool)
            {
                worker.Kill();
            }
        }
    }
}