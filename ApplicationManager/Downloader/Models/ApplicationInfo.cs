using System.Text.Json.Serialization;

namespace ApplicationManager.Downloader.Models
{
    public class ApplicationInfo : IDisplayable
    {
        public string Name { get; set; }
        public string Version { get; set; }

        [JsonConstructor]
        public ApplicationInfo()
        {
        }

        internal ApplicationInfo(IDisplayable source)
        {
            Name = source.Name;
            Version = source.Version;
        }
    }
}
