using Flintr_lib.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flintr_Runner.WorkerHelpers
{
    public class VariableStore
    {
        private Dictionary<string, object> varList;
        private TCPClient tCPClient;

        public VariableStore(TCPClient connection)
        {
            varList = new Dictionary<string, object>();
            tCPClient = connection;
        }

        public void AddVariable(string name, object obj)
        {
            if (name == null) throw new ArgumentNullException(nameof(name), "Variable name cannot be NULL.");
            if (!varList.ContainsKey(name)) throw new ArgumentException($"Distributed variable with name {name} does not exist in local datastore.", "name");

            lock (varList)
            {
                // Add the variable to the local repository.
                varList.Add(name, obj);
            }
            // Notify the remote manager that we have a copy of the variable.
            tCPClient.Send($"VAR REGISTER {name}");
        }

        public void RemoveVariable(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name), "Variable name cannot be NULL.");
            if (!varList.ContainsKey(name)) throw new ArgumentException($"Distributed variable with name {name} does not exist in local datastore.", "name");

            lock (varList)
            {
                // Remove the variable from the local repository.
                varList.Remove(name);
            }
            // Notify the remote manager that we no longer have a copy of the variable.
            tCPClient.Send($"VAR DEREGISTER {name}");
        }

        public T GetVariable<T>(string name)
        {
            if (name == null || name == "") throw new ArgumentException("Variable parameter 'name' cannot be NULL or empty.", nameof(name));

            if (varList.ContainsKey(name))
            {
                return getLocalVariable<T>(name);
            }

            try
            {
                return (T)getRemoteVariable<T>(name);
            }
            catch (IOException e)
            {
                throw new IOException($"An internal error occurred while retrieving datastore object {name} of type {typeof(T).Name}.", e);
            }
            catch (KeyNotFoundException e)
            {
                throw new KeyNotFoundException($"Variable with name '{name}' was not found locally or remotely.");
            }
        }

        private T getLocalVariable<T>(string name)
        {
            object result;

            if (!varList.TryGetValue(name, out result)) throw new KeyNotFoundException($"Variable with name '{name}' was not found locally.");

            return (T)result;
        }

        /// <summary>
        /// Retrieves the value of a variable from a remote Flintr datastore within the cluster.
        /// </summary>
        /// <typeparam name="T">Type of variable to be retrieved that implements ISerializable.</typeparam>
        /// <param name="name">Name of variable to be retrieved.</param>
        /// <returns>Value of remote variable.</returns>
        /// <exception cref="IOException">When an internal network error occurs during the request.</exception>
        /// <exception cref="KeyNotFoundException">When the remote key was not found in the cluster-wide datastore.</exception>
        private T getRemoteVariable<T>(string name)
        {
            T result;
            try
            {
                tCPClient.Send($"VAR RETRIEVE {name}");
                result = tCPClient.ReceiveObject<T>();
            }
            catch (Exception e)
            {
                throw new IOException($"An internal error occurred while retrieving network object {name} of type {typeof(T).Name}.", e);
            }

            if (result == null) throw new KeyNotFoundException($"Variable with name '{name}' was not found remotely.");

            return result;
        }
    }
}