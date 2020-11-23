using System;
using System.Text.Json.Serialization;

namespace ApplicationManager.Downloader.Models
{
    public class ApplicationInfo : IDisplayable
    {
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NameChanged?.Invoke(this, value);
            }
        }
        private string _name;

        public string Version { get; set; }

        protected event EventHandler<string> NameChanged;

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
