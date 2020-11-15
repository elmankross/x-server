using System;

namespace ApplicationManager.Hasher.Exceptions
{
    public class InvalidStreamTypeException : Exception
    {
        internal InvalidStreamTypeException(string message)
            : base(message)
        {

        }
    }
}
