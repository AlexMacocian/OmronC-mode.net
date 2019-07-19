using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C_mode.net.Exceptions
{
    /// <summary>
    /// Exception related to length of command.
    /// </summary>
    public class LengthException : Exception
    {
        public LengthException()
        {
        }

        public LengthException(string message) : base(message)
        {
        }

        public LengthException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LengthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
