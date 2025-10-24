using System;

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