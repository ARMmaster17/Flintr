using System;
using System.Runtime.Serialization;

namespace Flintr_Runner.Exceptions
{
    [Serializable]
    internal class WorkerDoesNotExistException : Exception
    {
        public WorkerDoesNotExistException()
        {
        }

        public WorkerDoesNotExistException(string message) : base(message)
        {
        }

        public WorkerDoesNotExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WorkerDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}