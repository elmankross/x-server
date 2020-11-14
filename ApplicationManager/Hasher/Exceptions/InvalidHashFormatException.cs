using System;

namespace ApplicationManager.Hasher.Exceptions
{
    public class InvalidHashFormatException : Exception
    {
        internal InvalidHashFormatException(string message)
            : base(message)
        {
        }
    }
}
