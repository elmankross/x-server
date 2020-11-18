using ApplicationManager.Identifier.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Executor
{
    public class Manager
    {
        private readonly ILogger _logger;
        private readonly Identifier _identifier;
        private readonly Tasker.Manager _tasker;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tasker"></param>
        public Manager(ILogger<Manager> logger, Tasker.Manager tasker)
        {
            _logger = logger;
            _tasker = tasker;
            _identifier = new Identifier();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="executable"></param>
        /// <returns></returns>
        public Task<Identity> ExecuteAsync(Downloader.Models.IExecutable executable)
        {
            var before = executable.Exec.Hooks?.Before != null
                ? GetProcessInfo(executable.BaseDirectory, executable.Exec.Hooks.Before)
                : null;
            var after = executable.Exec.Hooks?.After != null
                ? GetProcessInfo(executable.BaseDirectory, executable.Exec.Hooks.After)
                : null;
            var main = GetProcessInfo(executable.BaseDirectory, executable.Exec);

            if (executable.Exec.Hooks?.Before != null)
            {
                return StartTaskAsync(
                    () => (_identifier.GetNext(e => e.ExecuteHookBefore), before),
                    () => (_identifier.GetNext(e => e.ExecuteMain), main),
                    () => (_identifier.GetNext(e => e.ExecuteHookAfter), after));
            }
            else
            {
                return StartTaskAsync(
                    () => (_identifier.GetNext(e => e.ExecuteMain), main),
                    () => (_identifier.GetNext(e => e.ExecuteHookAfter), after),
                    null);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task TerminateAsync(Identity id)
        {
            return _tasker.StopAsync(id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private async Task<Identity> StartTaskAsync(
            Func<(Identity, ProcessStartInfo)> before,
            Func<(Identity, ProcessStartInfo)> current,
            Func<(Identity, ProcessStartInfo)> after)
        {
            var (id, info) = before();
            if (info == null)
            {
                return default;
            }

            _logger.LogDebug("Starting new process...");
            var mainProcess = Process.Start(info);
            _logger.LogDebug("Process was been started.");

            var token = new CancellationTokenSource();
            token.Token.Register(() => KillProcess(mainProcess));

            var taskId = await _tasker.StartAsync(async () =>
            {
                mainProcess.ErrorDataReceived += Process_ErrorDataReceived;
                mainProcess.Exited += Process_Exited;
                mainProcess.BeginErrorReadLine();

                while (!mainProcess.StandardOutput.EndOfStream)
                {
                    var output = await mainProcess.StandardOutput.ReadLineAsync();
                    if (!string.IsNullOrEmpty(output))
                    {
                        _logger.LogDebug(output);
                    }
                }

                await mainProcess.WaitForExitAsync(token.Token);

                mainProcess.CancelErrorRead();
                mainProcess.ErrorDataReceived -= Process_ErrorDataReceived;

                var subId = await StartTaskAsync(current, after, null);
                if (subId != new Identity())
                {
                    id.AddChild(subId);
                }

                _logger.LogDebug("Process was been executed.");
            }, token);

            id.AddChild(taskId);
            return id;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        private void KillProcess(Process process)
        {
            using (_logger.BeginScope(new { processId = process.Id }))
            {
                _logger.LogDebug("Killing the process...");
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
                _logger.LogDebug("Process was been killed.");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Exited(object sender, EventArgs e)
        {
            _logger.LogInformation("Process exited with {@args}.", e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger.LogError(e.Data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static ProcessStartInfo GetProcessInfo(string baseDir, Downloader.Models.ApplicationExecRoot executable)
        {
            var args = GetArgsString(executable.Args);
            // TODO: Move to separate place with dictionary of all keys?
            args = args.Replace("{BaseDir}", baseDir);
            var execPath = Path.Combine(baseDir, executable.Binary);
            return new ProcessStartInfo
            {
                FileName = execPath,
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
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetArgsString(Downloader.Models.ApplicationExecArgs args)
        {
            var sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append("--").Append(arg.Key);
                if (!string.IsNullOrEmpty(arg.Value))
                {
                    sb.Append("=").Append(arg.Value);
                }
                sb.Append(" ");
            }
            var result = sb.ToString();
            sb.Clear();
            return result;
        }
    }
}
