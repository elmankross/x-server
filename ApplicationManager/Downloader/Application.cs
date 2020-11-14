using System;

namespace ApplicationManager.Downloader
{
    /// <summary>
    /// Describe information about application
    /// </summary>
    public class Application
    {
        public Bitness Bitness { get; set; }
        public string Version { get; set; }
        public string Hash { get; set; }
        public Uri Url { get; set; }
        public string Exec { get; set; }
        public string Check { get; set; }
    }

    public enum Bitness
    {
        x64,
        x86
    }
}
