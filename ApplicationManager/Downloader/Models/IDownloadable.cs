using System;

namespace ApplicationManager.Downloader.Models
{
    public interface IDownloadable
    {
        string Hash { get; }
        Uri Url { get; }
        string[] Dependencies { get; }
    }
}
