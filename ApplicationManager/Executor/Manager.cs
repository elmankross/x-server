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
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="executable"></param>
        /// <returns></returns>
        public Task<Guid> ExecuteAsync(Downloader.Models.IExecutable executable)
        {
            if (executable.Exec.Hooks?.Before != null)
            {
                var beforeHook = GetProcessInfo(executable.BaseDirectory, executable.Exec.Hooks.Before);
                return StartTaskAsync(beforeHook,
                    () => GetProcessInfo(executable.BaseDirectory, executable.Exec),
                    () => GetProcessInfo(executable.BaseDirectory, executable.Exec.Hooks?.After));
            }
            else
            {
                var main = GetProcessInfo(executable.BaseDirectory, executable.Exec);
                return StartTaskAsync(main,
                    () => GetProcessInfo(executable.BaseDirectory, executable.Exec.Hooks?.After),
                    null);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task TerminateAsync(Guid id)
        {
            return _tasker.StopAsync(id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private Task<Guid> StartTaskAsync(ProcessStartInfo before, Func<ProcessStartInfo> main, Func<ProcessStartInfo> after)
        {
            _logger.LogDebug("Starting new process...");
            var mainProcess = Process.Start(before);
            _logger.LogDebug("Process was been started.");

            var token = new CancellationTokenSource();
            token.Token.Register(() => mainProcess.Kill());

            return _tasker.StartAsync(async () =>
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

                _logger.LogDebug("Process was been executed.");

                var mainProc = main?.Invoke();
                if (mainProc != null)
                {
                    await StartTaskAsync(mainProc, after, null);
                }
            }, token);
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
            if (executable == null)
            {
                return null;
            }

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
