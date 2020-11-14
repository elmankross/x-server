using System.IO;

namespace ApplicationManager.Downloader.Models
{
    public interface IFileInfo
    {
        public string FullName { get; }
        public Stream Stream { get; }
    }
}
