using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CustomAutoAdapterMapper.Exceptions
{
    public class RootKeyPropertyNullException : Exception
    {
        public RootKeyPropertyNullException()
        {
        }

        public RootKeyPropertyNullException(string message) : base(message)
        {
        }

        public RootKeyPropertyNullException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
