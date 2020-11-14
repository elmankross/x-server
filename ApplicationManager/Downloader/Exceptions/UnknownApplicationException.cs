using System;

namespace ApplicationManager.Downloader.Exceptions
{
    public class UnknownApplicationException : Exception
    {
        public string Application { get; }

        internal UnknownApplicationException(string application, string message)
            : base(message)
        {
            Application = application;
        }
    }
}
