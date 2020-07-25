using Flintr_Runner.Configuration;
using Flintr_Runner.Pool;
using Flintr_Runner.Runners;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flintr_Runner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RuntimeConfiguration runtimeConfiguration = new RuntimeConfiguration();
            // For now run in standalone mode.
            RunStandalone(runtimeConfiguration);
        }

        public static void RunStandalone(RuntimeConfiguration runtimeConfiguration)
        {
            bool shouldRun = true;
            // Create a manager thread.
            Manager manager = new Manager(runtimeConfiguration);
            // Setup the Manager thread.
            manager.Setup(runtimeConfiguration);
            // Start the Manager service thread.
            Task managerThread = Task.Run(() => manager.RunAsync());

            // Wait a few seconds for the manager to get set up.
            Thread.Sleep(1000);

            // Set up the workers.
            WorkerPool workerPool = new WorkerPool(runtimeConfiguration);
            workerPool.SetupAllWorkers(runtimeConfiguration);
            workerPool.RunAllWorkersAsync();
            Console.CancelKeyPress += (s, e) =>
            {
                runtimeConfiguration.GetLoggerInstance().Error("Runner", "Overwatch", "Console kill command received. Forcing shutdown.");
                workerPool.KillAllWorkers();
                shouldRun = false;
            };
            Task.WaitAll(managerThread);
        }
    }
}