using System.Collections.Generic;

namespace ApplicationManager.Downloader.Models
{
    public class ApplicationExecRoot
    {
        public string Binary { get; set; }
        public ApplicationExecArgs Args { get; set; }
    }

    public class ApplicationExec : ApplicationExecRoot
    {
        public ApplicationExecHooks Hooks { get; set; }
    }


    public class ApplicationExecArgs : Dictionary<string, string>
    {

    }


    public class ApplicationExecHooks
    {
        public ApplicationExecRoot Before { get; set; }
        public ApplicationExecRoot After { get; set; }
    }
}
