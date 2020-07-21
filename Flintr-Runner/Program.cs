using Flintr_Runner.Configuration;
using Flintr_Runner.Pool;
using Flintr_Runner.Runners;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flintr_Runner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RuntimeConfiguration runtimeConfiguration = new RuntimeConfiguration();
            // For now run in standalone mode.
            RunStandalone(runtimeConfiguration);
        }

        private static void RunStandalone(RuntimeConfiguration runtimeConfiguration)
        {
            bool shouldRun = true;
            // Create a manager thread.
            Manager manager = new Manager(runtimeConfiguration);
            // Setup the Manager thread.
            manager.Setup(runtimeConfiguration);
            // Start the Manager service thread.
            Task managerThread = Task.Run(() => manager.RunAsync());
            // Set up the workers.
            WorkerPool workerPool = new WorkerPool(runtimeConfiguration);
            workerPool.SetupAllWorkers(runtimeConfiguration);
            //workerPool.RunAllWorkersAsync();
            Console.CancelKeyPress += (s, e) =>
            {
                runtimeConfiguration.GetLoggerInstance().Error("Console kill command received. Forcing shutdown.");
                workerPool.KillAllWorkers();
                shouldRun = false;
            };
            Task.WaitAll(managerThread);
        }
    }
}