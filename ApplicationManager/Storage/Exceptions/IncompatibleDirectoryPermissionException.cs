using System;

namespace ApplicationManager.Storage.Exceptions
{
    public class IncompatibleDirectoryPermissionException : Exception
    {
        public string Path { get; }

        internal IncompatibleDirectoryPermissionException(string path, string message)
            : base(message)
        {
            Path = path;
        }
    }
}
