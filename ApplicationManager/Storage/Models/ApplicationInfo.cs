using System.Text.Json.Serialization;

namespace ApplicationManager.Storage.Models
{
    public class ApplicationInfo : Downloader.Models.ApplicationInfo
    {
        [JsonInclude]
        public InstallationState InstallationState { get; internal set; }

        [JsonInclude]
        public ExecutionState ExecutionState { get; internal set; }

        [JsonConstructor]
        internal ApplicationInfo()
        {
        }

        internal ApplicationInfo(Downloader.Models.IDisplayable source)
            : base(source)
        {
        }
    }

    public enum InstallationState
    {
        NotInstalled,
        Installed,
        Installing
    }

    public enum ExecutionState
    {
        NotExecuted,
        Executing,
        Erorred
    }
}
