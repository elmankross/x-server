using System;

namespace ApplicationManager.Storage.Exceptions
{
    public class InvalidApplicationSignatureException : Exception
    {
        internal InvalidApplicationSignatureException(string message)
            : base(message)
        {

        }
    }
}
