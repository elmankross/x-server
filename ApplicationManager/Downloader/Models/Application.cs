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
    public class Application : ApplicationInfo, IApplication
    {
        public Bitness Bitness { get; set; }
        public string Hash { get; set; }
        public Uri DownloadUrl { get; set; }
        public string[] Dependencies { get; set; }
        public ApplicationExec Exec { get; set; }
        public Uri WebUrl { get; set; }


        [JsonConstructor]
        public Application()
        {
            Dependencies = new string[0];
            NameChanged += Application_NameChanged;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_NameChanged(object sender, string name)
        {
            Exec ??= new ApplicationExec();
            Exec.Env.Add(ApplicationExecEnv.APPLICATION_NAME_KEY, name);
            Exec.Hooks?.Before?.Env.Add(ApplicationExecEnv.APPLICATION_NAME_KEY, name);
            Exec.Hooks?.After?.Env.Add(ApplicationExecEnv.APPLICATION_NAME_KEY, name);
            NameChanged -= Application_NameChanged;
        }
    }

    public enum Bitness
    {
        x64,
        x86
    }
}
