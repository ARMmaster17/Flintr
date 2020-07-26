using Flintr_Runner.Configuration;
using Flintr_Runner.ManagerHelpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Flintr_Runner_Test
{
    public class UnitWorkerRegistrationPoolTests
    {
        private WorkerRegistrationPool workerRegistrationPool;
        private RuntimeConfiguration runtimeConfiguration;

        [SetUp]
        public void Setup()
        {
            runtimeConfiguration = new RuntimeConfiguration();

            workerRegistrationPool = new WorkerRegistrationPool(runtimeConfiguration);
        }

        [Test]
        public void TestWorkerRegistration()
        {
            WorkerRegistration workerRegistration = workerRegistrationPool.RegisterNewWorker();
            Assert.IsNotNull(workerRegistration);
            Assert.AreEqual(workerRegistration.Name, "worker-1");
            Assert.AreEqual(workerRegistration.Port, runtimeConfiguration.GetManagerComPort() + 1);
            Assert.IsNull(workerRegistration.ClientServer);
            Assert.IsNotNull(workerRegistration.LastHeartBeat);
        }

        [Test]
        public void TestWorkerRegistrationPool()
        {
            WorkerRegistration workerRegistration = workerRegistrationPool.RegisterNewWorker();

            List<WorkerRegistration> registrationList = workerRegistrationPool.GetRegistrationPool();
            Assert.AreEqual(registrationList.Count, 1);
            Assert.AreSame(registrationList[0], workerRegistration);
        }

        [Test]
        public void TestWorkerNonDeadPool()
        {
            WorkerRegistration workerRegistration = workerRegistrationPool.RegisterNewWorker();

            List<WorkerRegistration> registrationList = workerRegistrationPool.GetNonDeadWorkerPool();
            Assert.AreEqual(registrationList.Count, 1);
            Assert.AreSame(registrationList[0], workerRegistration);
        }

        [Test]
        public void TestDeadWorkerNotInNonDeadPool()
        {
            DateTime deadTime = DateTime.Now.Subtract(new TimeSpan(0, 0, runtimeConfiguration.GetWorkerHeartbeatTimeout() + 1));
            WorkerRegistration workerRegistration = new WorkerRegistration("test_name", null, 1000, deadTime);

            workerRegistrationPool.AddToPool(workerRegistration);

            List<WorkerRegistration> registrationList = workerRegistrationPool.GetNonDeadWorkerPool();
            Assert.AreEqual(registrationList.Count, 0);
            Assert.That(!registrationList.Contains(workerRegistration));
        }

        [Test]
        public void TestManualAddWorkerToPool()
        {
            WorkerRegistration workerRegistration = new WorkerRegistration("test_name", null, 1000, DateTime.Now);

            workerRegistrationPool.AddToPool(workerRegistration);

            List<WorkerRegistration> registrationList = workerRegistrationPool.GetRegistrationPool();
            Assert.AreEqual(registrationList.Count, 1);
            Assert.AreSame(registrationList[0], workerRegistration);
        }
    }
}