using System;
using System.Collections.Generic;
using System.Net;
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
        static FlintrInstance flintrInstance;

        private static void Main(string[] args)
        {
            
            string address = "127.0.0.1";
            int port = 3999;
            if (promptMenu("Run Flintr in standalone or remote mode?", "Standalone Mode", "Remote Mode") == 0)
            {
                Console.WriteLine("Waiting for Flintr to start..");
                Task.Run(() => Flintr_Runner.Program.RunStandalone(new Flintr_Runner.Configuration.RuntimeConfiguration()));
                Thread.Sleep(10000);
            }
            else
            {
                address = promptUser("IP Address of Flinter Manager:", "127.0.0.1");
                port = Convert.ToInt32(promptUser("Port of Flinter Manager API:", "3999"));
            }
            Console.WriteLine("Connecting...");
            flintrInstance = new FlintrInstance(IPAddress.Parse(address), port);

            while(true)
            {
                switch(promptMenu("Select a task", "Administration", "Send Ad-Hoc Job", "Exit"))
                {
                    case 0:
                        MenuAdministration();
                        break;
                    case 1:
                        MenuAdHocJob();
                        break;
                    case 2:
                        Console.WriteLine("Quitting...");
                        return;
                    default:
                        Console.WriteLine("Invalid input, please try again.");
                        break;
                }
            }
        }

        private static void MenuAdHocJob()
        {
            while (true)
            {
                switch (promptMenu("Select a job to send", "Echo", "Return"))
                {
                    case 0:
                        string param = promptUser("Enter a string to display", "default");
                        flintrInstance.QueueRawJob(new EchoJob(JobStrategy.RunOnAll, param));
                        return;
                    case 1:
                        return;
                    default:
                        Console.WriteLine("Invalid input, please try again.");
                        break;
                }
            }
        }

        private static void MenuAdministration()
        {
            while (true)
            {
                switch (promptMenu("Select a task", "List Workers", "Return"))
                {
                    case 0:
                        List<WorkerDetail> wds = flintrInstance.GetAllWorkerDetails();
                        foreach (WorkerDetail wd in wds)
                        {
                            Console.WriteLine("{0}\t{1}\t{2}", wd.AssignedPort, wd.Name, "online");
                        }
                        break;
                    case 2:
                        return;
                    default:
                        Console.WriteLine("Invalid input, please try again.");
                        break;
                }
            }
        }

        private static string promptUser(string prompt, string defaultValue)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine() ?? defaultValue;
        }

        private static int promptMenu(string prompt, params string[] options)
        {
            while(true)
            {
                Console.WriteLine(prompt);
                for(int i = 0; i < options.Length; i++)
                {
                    Console.WriteLine("{0}) {1}", i, options[i]);
                }
                string rawOption = promptUser("Enter an option:", "0");
                bool conversionFailed = false;
                int option = -1;
                try
                {
                    option = Convert.ToInt32(rawOption);
                }
                catch
                {
                    conversionFailed = true;
                }
                if (conversionFailed || option < 0 || option >= options.Length)
                {
                    Console.WriteLine("Invalid option. Please enter a valid value (0-{0}).", options.Length);
                    continue;
                }
                return option;
            }
        }
    }
}