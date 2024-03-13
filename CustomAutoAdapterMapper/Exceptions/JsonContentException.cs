using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CustomAutoAdapterMapper.Exceptions
{
    public class JsonContentException : Exception
    {
        public JsonContentException()
        {
        }

        public JsonContentException(string message) : base(message)
        {
        }

        public JsonContentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
