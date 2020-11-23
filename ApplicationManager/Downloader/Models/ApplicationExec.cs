using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ApplicationManager.Downloader.Models
{
    public class ApplicationExecRoot
    {
        public string Binary { get; set; }
        public ApplicationExecArgs Args { get; set; }
        public ApplicationExecEnv Env { get; set; }


        [JsonConstructor]
        public ApplicationExecRoot()
        {
            Args = new ApplicationExecArgs();
            Env = new ApplicationExecEnv();
        }
    }

    public class ApplicationExec : ApplicationExecRoot
    {
        public ApplicationExecHooks Hooks { get; set; }
    }


    public class ApplicationExecArgs : HashSet<string>
    {
        public override string ToString()
        {
            if(Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendJoin(' ', this);
            return sb.ToString();
        }
    }


    public class ApplicationExecEnv : Dictionary<string, string>
    {
        internal const string APPLICATION_NAME_KEY = ":APP";
    }


    public class ApplicationExecHooks
    {
        public ApplicationExecRoot Before { get; set; }
        public ApplicationExecRoot After { get; set; }
    }
}
