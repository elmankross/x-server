using System;

namespace ApplicationManager.Downloader.Exceptions
{
    public class InvalidApplicationSignatureException : Exception
    {
        internal InvalidApplicationSignatureException(string message)
            : base(message)
        {

        }
    }
}
