using Flintr_lib.Jobs;
using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Flintr_Runner.Logging;

namespace Flintr_Runner.ManagerHelpers.Dispatch
{
    public class JobDispatchManager
    {
        private WorkerRegistrationPool workerPool;
        private Logger sharedLogger;
        private int nextJobId;
        private int nextTaskId;
        private List<DispatchedTask> dispatchedTasks;
        private Random random;
        private Queue<Job> jobQueue;

        public JobDispatchManager(RuntimeConfiguration runtimeConfiguration, WorkerRegistrationPool workerRegistrationPool)
        {
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            workerPool = workerRegistrationPool;
            nextJobId = -1;
            nextTaskId = -1;
            dispatchedTasks = new List<DispatchedTask>();
            random = new Random();
            jobQueue = new Queue<Job>();
        }

        public void QueueJob(Job job)
        {
            jobQueue.Enqueue(job);
            sharedLogger.Msg("Manager", "Job Dispatcher", $"Job of type {job.GetType().Name} was successfully enqueued.");
            if (!workersAvailable())
            {
                sharedLogger.Warning("Manager", "Job Dispatcher", "A job was queued but no workers are available!");
            }
        }

        public void ProcessJobQueue()
        {
            if (workersAvailable())
            {
                while (jobQueue.Count > 0)
                {
                    DispatchJob(jobQueue.Dequeue());
                }
            }
        }

        public DispatchedJob DispatchJob(Job job)
        {
            if (job.strategy == JobStrategy.RunOnAll) return dispatchToAll(job);
            else if (job.strategy == JobStrategy.RunOnOne) return dispatchToOne(job);
            else return null;
        }

        private DispatchedJob dispatchToAll(Job job)
        {
            return dispatchToSpecified(workerPool.GetNonDeadWorkerPool(), job);
        }

        private DispatchedJob dispatchToOne(Job job)
        {
            List<WorkerRegistration> availableWorkers = workerPool.GetNonDeadWorkerPool();
            List<WorkerRegistration> selectedWorker = new List<WorkerRegistration>();
            selectedWorker.Add(availableWorkers[random.Next(0, availableWorkers.Count)]);
            return dispatchToSpecified(selectedWorker, job);
        }

        private DispatchedJob dispatchToSpecified(List<WorkerRegistration> workers, Job job)
        {
            int jobId = getNextJobId();
            if (workers.Count == 0)
            {
                sharedLogger.Error("Manager", "Job Dispatcher", "No workers available to accept dispatched job!");
                return null;
            }
            List<DispatchedTask> jobSpecificTasksDispatched = new List<DispatchedTask>();
            foreach (WorkerRegistration worker in workers)
            {
                if (worker == null || worker.ClientServer == null)
                {
                    if (workers.Count == 1)
                    {
                        sharedLogger.Error("Manager", "Job Dispatcher", $"Worker '{worker.Name}' has a faulty registration. Queuing job again.");
                        QueueJob(job);
                        return null;
                    }
                    else
                    {
                        sharedLogger.Warning("Manager", "Job Dispatcher", $"Worker '{worker.Name}' has a faulty registration. Skipping...");
                    }
                }
                int taskId = getNextTaskId();
                worker.ClientServer.Send($"EXECUTE[{taskId}] {job.GetType().AssemblyQualifiedName}");
                worker.ClientServer.SendObject(job);
                DispatchedTask taskTracker = new DispatchedTask(taskId, worker.Name);
                jobSpecificTasksDispatched.Add(taskTracker);
                dispatchedTasks.Add(taskTracker);
                sharedLogger.Debug("Manager", "Job Dispatcher", $"Task ID {taskId} of Job ID {jobId} was dispatched to {worker.Name}.");
            }
            sharedLogger.Msg("Manager", "Job Dispatcher", $"Job of type {job.GetType().Name} with ID {jobId} was dispatched.");
            return new DispatchedJob(jobSpecificTasksDispatched, job, jobId);
        }

        private int getNextJobId()
        {
            nextJobId++;
            return nextJobId;
        }

        private int getNextTaskId()
        {
            nextTaskId++;
            return nextTaskId;
        }

        private bool workersAvailable()
        {
            return workerPool.GetNonDeadWorkerPool().Count > 0;
        }
    }
}