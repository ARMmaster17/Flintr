using Flintr_lib.Communication;
using Flintr_Runner.Configuration;
using System.Net;
using Flintr_Runner.Logging;
using System.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Flintr_Runner.ManagerHelpers
{
    /// <summary>
    /// Manages the registration pool of workers.
    /// </summary>
    public class WorkerRegistrationManager
    {
        private TCPServer tcpServer;
        private bool shouldListen;
        private Logger sharedLogger;
        private WorkerRegistrationPool registrationPool;
        private IPAddress managerBindIP;
        private RuntimeConfiguration runtimeConfiguration;

        /// <summary>
        ///Initializes the manager with specified settings.
        /// </summary>
        /// <param name="runtimeConfiguration">Active runtime configuration settings for this session.</param>
        /// <param name="workerRegistrationPool">Configured pool of workers to manage.</param>
        /// <exception cref="ArgumentException">When invalid IP/port bindings are configured.</exception>
        public WorkerRegistrationManager(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool)
        {
            managerBindIP = runtimeConfiguration.GetManagerBindAddress();
            try
            {
                tcpServer = new TCPServer(managerBindIP, runtimeConfiguration.GetManagerComPort());
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Invalid IP or port setting for the Worker Registration Manager. Check settings and try again.", e);
            }
            catch (IOException e)
            {
                throw new IOException("The Worker Registration Service encountered an error while binding to the configured IP/port configuration. Ensure that you have the correct permissions to use the port and that the requested port is not already in use by another process.", e);
            }

            shouldListen = false;
            registrationPool = workerRegistrationPool;
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            this.runtimeConfiguration = runtimeConfiguration;
        }

        /// <summary>
        /// De-initializer that stops the loop on all sub-tasks.
        /// </summary>
        ~WorkerRegistrationManager()
        {
            StopListening();
        }

        /// <summary>
        /// Starts a blocking loop that listens for new worker registrations.
        /// </summary>
        public void ListenAsync()
        {
            shouldListen = true;
            while (shouldListen)
            {
                TCPClient newClient;
                try
                {
                    newClient = tcpServer.WaitForNextConnection();
                }
                catch (IOException e)
                {
                    throw new IOException("An error occurred while listening for new worker registrations.", e);
                }
                Task.Run(() => startWorkerRegistrationProcess(newClient));
            }
        }

        /// <summary>
        /// Stops the asynchronous task that listens for new worker registrations.
        /// </summary>
        public void StopListening()
        {
            shouldListen = false;
        }

        /// <summary>
        /// Retrieves a slot in the registration pool for a new worker.
        /// </summary>
        /// <param name="infoString">List of capabilities and features of new worker (not implemented).</param>
        /// <returns>Container with newly assigned name and private communication port number.</returns>
        private WorkerRegistration getNewWorkerRegistrationInformation(string infoString)
        {
            WorkerRegistration newRegistration;
            lock (registrationPool)
            {
                newRegistration = registrationPool.RegisterNewWorker();
            }
            sharedLogger.Msg("Manager", "Worker Registration Service", $"New worker registration '{newRegistration.Name}' on port {newRegistration.Port}");
            return newRegistration;
        }

        /// <summary>
        /// Handles the registration of new workers and performs a hand-off to a private communication port
        /// </summary>
        /// <param name="newClient">Active connection from worker requesting registration.</param>
        private void startWorkerRegistrationProcess(TCPClient newClient)
        {
            WorkerRegistration registration = getNewWorkerRegistrationInformation(newClient.ReceiveObject<string>());
            try
            {
                newClient.SendObject<string>($"{registration.Port.ToString()}|{registration.Name}");
            }
            catch (ArgumentException e)
            {
                sharedLogger.Error("Manger", "Worker Registration Manager", $"Worker registration with new worker '{registration.Name}' failed.");
                sharedLogger.ErrorStackTrace("Manager", "Worker Registration Manager", new Exception("An error occurred while registering a new worker.", e));
                return;
            }
            catch (IOException e)
            {
                sharedLogger.Error("Manger", "Worker Registration Manager", $"Worker registration with new worker '{registration.Name}' failed.");
                sharedLogger.ErrorStackTrace("Manager", "Worker Registration Manager", new Exception($"A network error occurred while sending registration information for worker '{registration.Name}'.", e));
                return;
            }
            sharedLogger.Debug("Manager", "Worker Registration Service", $"Transferring {registration.Name} to new connection...");
            TCPServer newConnection;
            try
            {
                newConnection = new TCPServer(managerBindIP, registration.Port);
                registration.ClientServer = newConnection.WaitForNextConnection();
            }
            catch (ArgumentException e)
            {
                sharedLogger.Error("Manger", "Worker Registration Manager", $"Worker registration with new worker '{registration.Name}' failed.");
                sharedLogger.ErrorStackTrace("Manager", "Worker Registration Manager", new Exception("An error occurred while registering a new worker.", e));
                return;
            }
            catch (IOException e)
            {
                sharedLogger.Error("Manger", "Worker Registration Manager", $"Worker registration with new worker '{registration.Name}' failed.");
                sharedLogger.ErrorStackTrace("Manager", "Worker Registration Manager", new Exception($"A network error occurred while transferring worker '{registration.Name}' to new communication port.", e));
                return;
            }
            sharedLogger.Debug("Manager", "Worker Registration Service", $"Connection successful on new channel.");
        }
    }
}