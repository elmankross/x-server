using System.Text.Json.Serialization;

namespace ApplicationManager.Downloader.Models
{
    public class ApplicationInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }

        [JsonConstructor]
        public ApplicationInfo()
        {
        }

        internal ApplicationInfo(ApplicationInfo source)
        {
            Name = source.Name;
            Version = source.Version;
        }
    }
}
