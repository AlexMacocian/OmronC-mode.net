﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace C_mode.net.Exceptions
{
    public class FormatException : Exception
    {
        public FormatException()
        {
        }

        public FormatException(string message) : base(message)
        {
        }

        public FormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
