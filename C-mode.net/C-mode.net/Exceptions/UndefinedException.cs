using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C_mode.net.Exceptions
{
    /// <summary>
    /// Class for undefined exceptions.
    /// </summary>
    public class UndefinedException : Exception
    {
        public UndefinedException()
        {
        }

        public UndefinedException(string message) : base(message)
        {
        }

        public UndefinedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UndefinedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
