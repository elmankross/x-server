using System;

namespace ApplicationManager.Unboxer.Exceptions
{
    public class InvalidUnboxerTypeException : Exception
    {
        public string Type { get; }

        internal InvalidUnboxerTypeException(string type, string message)
            : base(message)
        {
            Type = type;
        }
    }
}
