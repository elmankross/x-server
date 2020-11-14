using System;

namespace ApplicationManager.Storage.Exceptions
{
    public class DirectoryNotExistsException : Exception
    {
        public string Path { get; }

        internal DirectoryNotExistsException(string path, string message)
            : base(message)
        {
            Path = path;
        }
    }
}
