using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C_mode.net.Exceptions
{
    /// <summary>
    /// Exception regarding CPU unit mismatch.
    /// </summary>
    public class CPUUnitException : Exception
    {
        public CPUUnitException()
        {
        }

        public CPUUnitException(string message) : base(message)
        {
        }

        public CPUUnitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CPUUnitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
