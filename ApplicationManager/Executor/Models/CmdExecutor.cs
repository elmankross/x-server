using System.Threading;

namespace ApplicationManager.Executor.Models
{
    internal class CmdExecutor : ProcessExecutor
    {
        internal const string COMMAND_PREFIX = ":cmd";
        protected override char ArgKeyPrefix { get; } = '/';

        internal CmdExecutor(
            CancellationToken cancellationToken,
            Downloader.Models.ApplicationExecRoot executable,
            string appBaseDirectory)
            : base(cancellationToken, OverrideExecutable(executable), appBaseDirectory)
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
