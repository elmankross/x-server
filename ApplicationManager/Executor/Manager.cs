using ApplicationManager.Identifier.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Executor
{
    public class Manager
    {
        public event EventHandler<Identity> Exited;
        public event EventHandler<Identity> Errored;

        private readonly ILogger _logger;
        private readonly Identifier _identifier;
        private readonly Tasker.Manager _tasker;
        private readonly ConcurrentDictionary<Identity, Identity> _identityMaps;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tasker"></param>
        public Manager(ILogger<Manager> logger, Tasker.Manager<Manager> tasker)
        {
            _logger = logger;
            _tasker = tasker;
            _identifier = new Identifier();
            _identityMaps = new ConcurrentDictionary<Identity, Identity>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="executable"></param>
        /// <returns></returns>
        public Task<Identity> ExecuteAsync(Storage.Models.StorageEnv storageEnv, Downloader.Models.IExecutable executable)
        {
            if (executable.Exec.Hooks?.Before != null)
            {
                return StartTaskAsync(
                    storageEnv,
                    () => (_identifier.GetNext(e => e.ExecuteHookBefore), executable.Exec.Hooks?.Before),
                    () => (_identifier.GetNext(e => e.ExecuteMain), executable.Exec),
                    () => (_identifier.GetNext(e => e.ExecuteHookAfter), executable.Exec.Hooks?.After));
            }
            else
            {
                return StartTaskAsync(
                    storageEnv,
                    () => (_identifier.GetNext(e => e.ExecuteMain), executable.Exec),
                    () => (_identifier.GetNext(e => e.ExecuteHookAfter), executable.Exec.Hooks?.After),
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
            using (_logger.BeginScope(id))
            {
                if (_identityMaps.TryGetValue(id, out var taskId))
                {
                    return _tasker.StopAsync(taskId);
                }
                else
                {
                    _logger.LogWarning("Can't find task's id. Ignoring action...");
                    return Task.CompletedTask;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private async Task<Identity> StartTaskAsync(
            Storage.Models.StorageEnv storageEnv,
            Func<(Identity, Downloader.Models.ApplicationExecRoot)> before,
            Func<(Identity, Downloader.Models.ApplicationExecRoot)> current,
            Func<(Identity, Downloader.Models.ApplicationExecRoot)> after)
        {
            var (id, info) = before();
            if (info == null)
            {
                return default;
            }

            var token = new CancellationTokenSource();

            _logger.LogDebug("Starting new process...");
            var exec = Models.Executor.Get(token.Token, info, storageEnv);
            _logger.LogDebug("Process was been started.");

            exec.OnError += (s, e) => Exec_OnError(s, e, id);
            exec.OnMessage += (s, e) => Exec_OnMessage(s, e, id);
            exec.OnExited += (s, e) => Exec_OnExited(s, e, id);

            token.Token.Register(() =>
            {
                exec.StopAsync().Wait();
                exec.Dispose();
            });

            var taskId = await _tasker.StartAsync(async () =>
            {
                try
                {
                    await exec.StartAsync();

                    var subId = await StartTaskAsync(storageEnv, current, after, null);
                    if (subId != new Identity())
                    {
                        id.AddChild(subId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There is an error in background task.");
                }
                finally
                {
                    exec.OnError -= (s, e) => Exec_OnError(s, e, id);
                    exec.OnMessage -= (s, e) => Exec_OnMessage(s, e, id);
                    exec.OnExited -= (s, e) => Exec_OnExited(s, e, id);

                    _logger.LogDebug("Process was been executed.");
                }
            }, token);

            if (!_identityMaps.TryAdd(id, taskId))
            {
                _logger.LogWarning("Can't add id mapping [Executor-Tasker]. Continue without it...");
            }
            return id;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        /// <param name="id"></param>
        private void Exec_OnExited(object _, EventArgs __, Identity id)
        {
            using (_logger.BeginScope(id))
            {
                _logger.LogDebug("Application was been exited.");
                Exited?.Invoke(this, id);
           }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="e"></param>
        /// <param name="id"></param>
        private void Exec_OnMessage(object _, string e, Identity id)
        {
            using (_logger.BeginScope(id))
            {
                _logger.LogInformation(e);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="e"></param>
        /// <param name="id"></param>
        private void Exec_OnError(object _, string e, Identity id)
        {
            using (_logger.BeginScope(id))
            {
                _logger.LogError(e);
                Errored?.Invoke(this, id);
            }
        }
    }
}
