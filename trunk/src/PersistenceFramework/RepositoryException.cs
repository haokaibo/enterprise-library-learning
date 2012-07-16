using System;
using System.Runtime.Serialization;

namespace PersistenceFramework
{
    public class RepositoryException : Exception
    {
        public RepositoryException()
            : base("Stock quote service error")
        {
        }

        public RepositoryException(string message)
            : base(message)
        {
        }

        protected RepositoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public RepositoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}