using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.ManagerHelpers.Dispatch
{
    public class JobDispatchManager
    {
        private WorkerRegistrationPool workerPool;
        private Logger.Logger sharedLogger;
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
        }

        public void ProcessJobQueue()
        {
            while (jobQueue.Count > 0)
            {
                DispatchJob(jobQueue.Dequeue());
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
            if (workers.Count == 0)
            {
                sharedLogger.Error("No workers available to accept dispatched job!");
                return null;
            }
            List<DispatchedTask> jobSpecificTasksDispatched = new List<DispatchedTask>();
            foreach (WorkerRegistration worker in workers)
            {
                int taskId = getNextTaskId();
                worker.ClientServer.Send(job.GetComLine(taskId, ""));
                DispatchedTask taskTracker = new DispatchedTask(taskId, worker.Name);
                jobSpecificTasksDispatched.Add(taskTracker);
                dispatchedTasks.Add(taskTracker);
            }
            return new DispatchedJob(jobSpecificTasksDispatched, job);
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
    }
}