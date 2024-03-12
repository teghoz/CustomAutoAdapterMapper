using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CustomAutoAdapterMapper.Exceptions
{
    public class RootKeyOptionNullException : Exception
    {
        public RootKeyOptionNullException()
        {
        }

        public RootKeyOptionNullException(string message) : base(message)
        {
        }

        public RootKeyOptionNullException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
