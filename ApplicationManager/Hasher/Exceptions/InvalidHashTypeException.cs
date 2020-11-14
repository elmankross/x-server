using System;

namespace ApplicationManager.Hasher.Exceptions
{
    public class InvalidHashTypeException : Exception
    {
        public string Type { get; }

        internal InvalidHashTypeException(string type, string message)
            : base(message)
        {
            Type = type;
        }
    }
}
