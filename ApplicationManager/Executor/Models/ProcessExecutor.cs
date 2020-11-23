using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Executor.Models
{
    internal class ProcessExecutor : Executor
    {
        private readonly ProcessStartInfo _info;
        private Process _process;

        internal ProcessExecutor(
            CancellationToken cancellationToken,
            Downloader.Models.ApplicationExecRoot executable,
            Storage.Models.StorageEnv env)
            : base(cancellationToken, env)
        {
            _info = new ProcessStartInfo
            {
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var args = ReplaceWithEnvironment(executable.Args.ToString());
            var command = ReplaceWithEnvironment(executable.Binary);

            foreach (var e in executable.Env)
            {
                var enValue = ReplaceWithEnvironment(e.Value);
                // FIXME: OMG! I am so sowwy(((
                foreach(var e1 in executable.Env)
                {
                    enValue = enValue.Replace(e1.Key, e1.Value);
                }
                _info.Environment.Add(e.Key, enValue);
                command = command.Replace(e.Key, e.Value);
                args = args.Replace(e.Key, e.Value);
            }

            _info.FileName = command;
            _info.Arguments = args;
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
        public override void Dispose()
        {
            _process.Dispose();
        }
    }
}
