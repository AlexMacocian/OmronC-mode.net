using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C_mode.net.Exceptions
{
    /// <summary>
    /// Entry number exception.
    /// </summary>
    public class EntryNumberException : Exception
    {
        public EntryNumberException()
        {
        }

        public EntryNumberException(string message) : base(message)
        {
        }

        public EntryNumberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntryNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
