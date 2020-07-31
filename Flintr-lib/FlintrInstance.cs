using Flintr_lib.Communication;
using Flintr_lib.Jobs;
using Flintr_lib.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Flintr_lib
{
    /// <summary>
    /// Represents and manages a connection to a Flintr Manager instance.
    /// </summary>
    public sealed class FlintrInstance
    {
        private TCPClient managerConnection;

        /// <summary>
        /// Initializes the FlintrInstance to connect to a localhost Manager API instance with default connection settings.
        /// </summary>
        /// <exception cref="IOException">When an internal network error occurs while establishing a connection with the Manager API interface.</exception>
        public FlintrInstance()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            int port = 3999;

            IPEndPoint flintrManager = new IPEndPoint(address, port); ;

            try
            {
                Connect(flintrManager);
            }
            catch (Exception e)
            {
                throw new IOException("An internal network error occurred while connecting to the localhost Flintr Manager API interface.", e);
            }
        }

        /// <summary>
        /// Initializes the FlintrInstance to connect to a specified Flintr Manager API with the specified connection settings.
        /// </summary>
        /// <param name="flintrManagerAddress">IP address of the Flintr Manager instance.</param>
        /// <param name="flintrExecPort">Port that the Flintr Manager API instance is listening for new connections.</param>
        /// <exception cref="ArgumentException">When invalid connection details are provided.</exception>
        /// <exception cref="IOException">When an internal network error occurs while establishing a connection with the Manager API interface.</exception>
        public FlintrInstance(IPAddress flintrManagerAddress, int flintrExecPort)
        {
            if (flintrManagerAddress == null) throw new ArgumentException("Flintr Manager IP address cannot be NULL.");
            if (flintrExecPort == 0) throw new ArgumentException("Flintr Manager API port cannot be NULL.");

            IPEndPoint flintrManager;
            try
            {
                flintrManager = new IPEndPoint(flintrManagerAddress, flintrExecPort);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Invalid connection details provided. Check values and try again.", e);
            }

            try
            {
                Connect(flintrManager);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Invalid connection details provided. Check values and try again.", e);
            }
            catch (IOException e)
            {
                throw new IOException("An internal network error occurred while initializing the Manager API interface.", e);
            }
        }

        /// <summary>
        /// Initializes the FlintrInstance to connect to a specified Flintr Manager API endpoint.
        /// </summary>
        /// <param name="flintrManager">Endpoint to connect to.</param>
        /// <exception cref="ArgumentException">When invalid connection details are provided.</exception>
        /// <exception cref="IOException">When an internal network error occurs while establishing a connection with the Manager API interface.</exception>
        public FlintrInstance(IPEndPoint flintrManager)
        {
            if (flintrManager == null) throw new ArgumentException("flintrManager connection details cannot be NULL.");
            try
            {
                Connect(flintrManager);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Invalid connection details provided. Check values and try again.", e);
            }
            catch (IOException e)
            {
                throw new IOException("An internal network error occurred while initializing the Manager API interface.", e);
            }
        }

        /// <summary>
        /// Connects to a Manager API interface at the given endpoint.
        /// </summary>
        /// <param name="flintrManager">Endpoint where Manager API is listening for new connections.</param>
        /// <exception cref="ArgumentException">When invalid connection details are provided.</exception>
        /// <exception cref="IOException">When an internal network error occurs while establishing a connection with the Manager API interface.</exception>
        public void Connect(IPEndPoint flintrManager)
        {
            if (flintrManager == null) throw new ArgumentException("Argument flintrManager cannot be NULL.");

            try
            {
                managerConnection = new TCPClient(flintrManager);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Invalid connection details provided. Check values and try again.", e);
            }
            catch (IOException e)
            {
                throw new IOException("An internal network error occurred while attempting to connect to the Manager API interface.", e);
            }
        }

        /// <summary>
        /// Returns a detailed report about the status of the specified worker (if it exists).
        /// </summary>
        /// <param name="workerName">Manager-assigned name of worker to perform lookup on.</param>
        /// <returns>Report of worker capabilities and status.</returns>
        /// <exception cref="ArgumentException">When supplied worker name is not valid.</exception>
        /// <exception cref="IOException">When an internal network occurs during request.</exception>
        public WorkerDetail GetWorkerDetails(string workerName)
        {
            try
            {
                managerConnection.Send($"REPORT LISTWORKER {workerName}");
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Invalid worker name was provided. Check details and try again.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"An internal network error occurred while requesting a worker status report.", e);
            }

            try
            {
                return managerConnection.ReceiveObject<WorkerDetail>();
            }
            catch (Exception e)
            {
                throw new IOException($"An internal network error occurred while receiving a worker status report.", e);
            }
        }

        public List<WorkerDetail> GetAllWorkerDetails()
        {
            try
            {
                managerConnection.Send("REPORT LISTALLWORKERS");
            }
            catch (ArgumentException e)
            {
                // TODO: Fill in exception catching.
            }
            catch (IOException e)
            {

            }

            try
            {
                return managerConnection.ReceiveObject<List<WorkerDetail>>();
            }
            catch (Exception e)
            {
                throw new IOException($"An internal network error occurred while receiving a worker list report.", e);
            }
        }

        /// <summary>
        /// Submits a job for immediate processing.
        /// </summary>
        /// <param name="job">Inherited instance of class Job to send for processing.</param>
        /// <returns>JobDetail that allows for checking for updates on job completion.</returns>
        /// <exception cref="ArgumentException">When invalid job details are provided.</exception>
        /// <exception cref="IOException">When an internal network error occurs while sending the job details.</exception>
        public JobDetail ExecuteRawJob(Job job)
        {
            if (job == null) throw new ArgumentException("Job cannot be NULL.");

            try
            {
                managerConnection.Send("EXECUTE");
                managerConnection.SendObject(job);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Invalid job details of job type {job.GetType().Name} were provided.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"An internal network error occurred while sending details of job {job.GetType().Name}.", e);
            }

            try
            {
                return managerConnection.ReceiveObject<JobDetail>();
            }
            catch (Exception e)
            {
                throw new IOException($"An error occurred while retrieving details of submitted job of type {job.GetType().Name}.", e);
            }
        }

        /// <summary>
        /// Adds a class of type Job to the job queue.
        /// </summary>
        /// <param name="job">Inherited instance of class Job to enqueue.</param>
        /// <exception cref="ArgumentException">When invalid job details are provided.</exception>
        /// <exception cref="IOException">When an internal network error occurs while sending the job details.</exception>
        public void QueueRawJob(Job job)
        {
            if (job == null) throw new ArgumentException("Job cannot be NULL.");

            try
            {
                managerConnection.Send($"QUEUEJOB");
                managerConnection.SendObject(job);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Invalid job details of job type {job.GetType().Name} were provided.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"An internal network error occurred while sending details of job {job.GetType().Name}.", e);
            }
        }
    }
}