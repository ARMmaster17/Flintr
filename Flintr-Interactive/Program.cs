using System;
using System.Threading;
using System.Threading.Tasks;
using Flintr_lib;
using Flintr_lib.Jobs;
using Flintr_lib.Reports;
using Flintr_Runner;

namespace Flintr_Interactive
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Flintr in standalone mode.");
            Task.Run(() => Flintr_Runner.Program.RunStandalone(new Flintr_Runner.Configuration.RuntimeConfiguration()));
            Thread.Sleep(10000);
            Console.WriteLine("Connecting to API at 127.0.0.1:3999");
            FlintrInstance instance = new FlintrInstance();
            Console.WriteLine("Connected");
            Console.WriteLine("Probing for first worker information.");
            WorkerDetail wd = instance.GetWorkerDetails("worker-1");
            Console.WriteLine($"Worker Info:\t{wd.Name}\t{wd.AssignedPort}\t{wd.LastReportedHeartbeat}");
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "") break;
                EchoJob ej = new EchoJob(JobStrategy.RunOnAll, input);
                instance.QueueRawJob(ej);
            }
        }
    }
}