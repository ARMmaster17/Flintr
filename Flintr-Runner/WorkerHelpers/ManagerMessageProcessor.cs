using Flintr_lib.Communication;
using Flintr_lib.Jobs;
using Flintr_Runner.Configuration;
using Flintr_Runner.Logging;
using Flintr_Runner.WorkerHelpers.Dispatch;
using System.Text.RegularExpressions;

namespace Flintr_Runner.WorkerHelpers
{
    public class ManagerMessageProcessor
    {
        private Logger logger;
        private string runnerName;
        private TaskDispatchManager taskDispatchManager;

        /// <summary>
        /// Default constructor. Initializes the message processor to dispatch tasks within the worker runner thread.
        /// </summary>
        /// <param name="runtimeConfiguration">Active application configuration.</param>
        /// <param name="runnerName">Assigned name of the worker runner instance.</param>
        public ManagerMessageProcessor(RuntimeConfiguration runtimeConfiguration, string runnerName, TaskDispatchManager taskDispatchManager)
        {
            logger = runtimeConfiguration.GetLoggerInstance();
            this.runnerName = runnerName;
            this.taskDispatchManager = taskDispatchManager;
        }

        /// <summary>
        /// Changes the assigned name of the worker node.
        /// </summary>
        /// <param name="runnerName">New assigned name of the worker node.</param>
        public void UpdateRunnerName(string runnerName)
        {
            this.runnerName = runnerName;
        }

        /// <summary>
        /// Matches a command to a known class and sends the full command there for further processing.
        /// </summary>
        /// <param name="rawCommand">Command as sent by manager instance.</param>
        /// <param name="client">Network socket that command was recieved from for further communication.</param>
        public void ProcessMessage(string rawCommand, TCPClient client)
        {
            if (Regex.IsMatch(rawCommand, @"^EXECUTE\[\d+\] .+$")) processExecuteCommand(rawCommand, client);
        }

        /// <summary>
        /// Handles the dispatching of job requests.
        /// </summary>
        /// <param name="rawCommand">Command as sent by manager instance.</param>
        /// <param name="client">Network socket that command was recieved from for further communication.</param>
        private void processExecuteCommand(string rawCommand, TCPClient client)
        {
            string jobID = getStringBetweenCharacters(rawCommand, '[', ']');
            Job job = client.ReceiveObject<Job>();
            taskDispatchManager.DispatchTask(jobID, job);
        }

        /// <summary>
        /// Helper function that returns a segment of a string between two control characters.
        /// </summary>
        /// <param name="input">String to examine.</param>
        /// <param name="beginning">Character to start the split at.</param>
        /// <param name="ending">Character to end the split at.</param>
        /// <returns>Substring between the two specified control characters.</returns>
        private static string getStringBetweenCharacters(string input, char beginning, char ending)
        {
            int startIndex = input.IndexOf(beginning);
            int endIndex = input.IndexOf(ending);
            return input.Substring(startIndex + 1, endIndex - startIndex - 1);
        }
    }
}