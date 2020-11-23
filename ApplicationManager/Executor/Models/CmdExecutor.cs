using System.Threading;

namespace ApplicationManager.Executor.Models
{
    internal class CmdExecutor : ProcessExecutor
    {
        internal const string COMMAND_PREFIX = ":cmd";

        internal CmdExecutor(
            CancellationToken cancellationToken,
            Downloader.Models.ApplicationExecRoot executable,
            Storage.Models.StorageEnv env)
            : base(cancellationToken, OverrideExecutable(executable), env)
        {

        }


        private static Downloader.Models.ApplicationExecRoot OverrideExecutable(
            Downloader.Models.ApplicationExecRoot exec)
        {
            exec.Binary = "cmd.exe";
            return exec;
        }
    }
}
