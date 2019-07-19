using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C_mode.net.Exceptions
{
    /// <summary>
    /// Exception in case of FCS error.
    /// </summary>
    public class FCSException : Exception
    {
        public FCSException()
        {
        }

        public FCSException(string message) : base(message)
        {
        }

        public FCSException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FCSException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
