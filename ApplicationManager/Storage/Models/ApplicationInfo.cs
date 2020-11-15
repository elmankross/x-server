using System.Text.Json.Serialization;

namespace ApplicationManager.Storage.Models
{
    public class ApplicationInfo : Downloader.Models.ApplicationInfo
    {
        [JsonInclude]
        public Status Status { get; internal set; }

        [JsonConstructor]
        internal ApplicationInfo()
        {
        }

        internal ApplicationInfo(Downloader.Models.ApplicationInfo source)
            : base(source)
        {
        }
    }

    public enum Status
    {
        NotInstalled,
        Installed,
        Installing
    }
}
