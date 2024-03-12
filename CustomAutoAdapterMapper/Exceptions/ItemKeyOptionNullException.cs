using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CustomAutoAdapterMapper.Exceptions
{
    public class ItemKeyOptionNullException : Exception
    {
        public ItemKeyOptionNullException()
        {
        }

        public ItemKeyOptionNullException(string message) : base(message)
        {
        }

        public ItemKeyOptionNullException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
