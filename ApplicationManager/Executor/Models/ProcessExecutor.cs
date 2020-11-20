using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Executor.Models
{
    internal class ProcessExecutor : Executor
    {
        protected virtual char ArgKeyPrefix { get; } = '-';
        protected virtual char ArgValPrefix { get; } = ' ';

        private readonly ProcessStartInfo _info;
        private Process _process;

        internal ProcessExecutor(
            CancellationToken cancellationToken,
            Downloader.Models.ApplicationExecRoot executable,
            string appBaseDirectory)
            : base(cancellationToken)
        {
            var args = GetArgsString(executable.Args);
            // TODO: Move to separate place with dictionary of all keys?
            args = args.Replace("{BaseDir}", appBaseDirectory);
            var command = executable.Binary.Replace("{BaseDir}", appBaseDirectory);
            _info = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }


        /// <summary>
        /// 
        /// </summary>
        internal override async Task StartAsync()
        {
            _process = Process.Start(_info);

            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;
            _process.BeginErrorReadLine();

            while (!_process.StandardOutput.EndOfStream)
            {
                var output = await _process.StandardOutput.ReadLineAsync();
                PushMessage(output);
            }

            await _process.WaitForExitAsync(CancellationToken);

            _process.CancelErrorRead();
            _process.ErrorDataReceived -= Process_ErrorDataReceived;
            _process.Exited -= Process_Exited;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override Task StopAsync()
        {
            _process.Kill(entireProcessTree: true);
            return _process.WaitForExitAsync(CancellationToken);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Exited(object sender, System.EventArgs e)
        {
            PushExited();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            PushError(e.Data);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string GetArgsString(Downloader.Models.ApplicationExecArgs args)
        {
            if (args == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(ArgKeyPrefix).Append(arg.Key);
                if (!string.IsNullOrEmpty(arg.Value))
                {
                    sb.Append(ArgValPrefix).Append(arg.Value);
                }
                sb.Append(' ');
            }
            var result = sb.ToString();
            sb.Clear();
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            _process.Dispose();
        }
    }
}
