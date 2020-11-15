using System.IO;

namespace ApplicationManager.Downloader.Models
{
    public class ApplicationFile
    {
        public Stream Stream { get; }
        public Unboxer.Manager Unboxer { get; }
        public Hasher.Manager Hasher { get; }

        internal ApplicationFile(Stream stream, Unboxer.Manager unboxer, Hasher.Manager hasher)
        {
            Stream = stream;
            Unboxer = unboxer;
            Hasher = hasher;
        }
    }
}
