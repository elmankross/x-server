using System;

namespace ApplicationManager.Downloader.Models
{
    public interface IDownloadable
    {
        string Hash { get; }
        Uri DownloadUrl { get; }
        string[] Dependencies { get; }
    }
}
