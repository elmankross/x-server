using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ApplicationManager.Downloader.Models
{
    /// <summary>
    /// Stores information about all available applications
    /// </summary>
    public class Applications : List<Application>
    {
        internal Dictionary<string, Application> Dictionary
        {
            get => _dictionary ??= this.ToDictionary(x => x.Name);
        }

        private Dictionary<string, Application> _dictionary;
    }


    /// <summary>
    /// Describe information about application
    /// </summary>
    public class Application : ApplicationInfo
    {
        public Bitness Bitness { get; set; }
        public string Hash { get; set; }
        public Uri Url { get; set; }
        public string Exec { get; set; }
        public Check Check { get; set; }
        public string[] Dependencies { get; set; }

        [JsonConstructor]
        public Application()
        {
        }

        internal Application(ApplicationInfo source)
            : base(source)
        {
        }
    }

    public enum Bitness
    {
        x64,
        x86
    }
}
