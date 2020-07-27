using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flintr_Runner.ManagerHelpers
{
    /// <summary>
    /// Handles the retrieval and lookup of local variables spread out among the worker nodes.
    /// </summary>
    public class DataStoreManager
    {
        private Dictionary<string, WorkerRegistration> varStorageLookupTable;

        public DataStoreManager()
        {
            varStorageLookupTable = new Dictionary<string, WorkerRegistration>();
        }

        public void ProcessCommand(string rawCommand, WorkerRegistration workerRegistration)
        {
            string specificCommand = Regex.Replace(rawCommand, @"^VAR ", "");
        }
    }
}