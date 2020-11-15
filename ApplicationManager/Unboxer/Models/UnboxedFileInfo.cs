using System.IO;

namespace ApplicationManager.Unboxer.Models
{
    public class UnboxedFileInfo
    {
        public string FullName { get; }
        public Stream Stream { get; }

        internal UnboxedFileInfo(string fullName, Stream stream)
        {
            FullName = fullName;
            Stream = stream;
        }
    }
}
