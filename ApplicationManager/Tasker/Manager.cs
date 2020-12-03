using ApplicationManager.Identifier.Models;
using ApplicationManager.Tasker.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Tasker
{
    /// <summary>
    /// Derives for different contexts
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class Manager<TContext> : Manager
    {
        public Manager(ILogger<Manager<TContext>> logger)
            :base(logger)
        {
        }
    }


    public abstract class Manager : IDisposable
    {
        private readonly ConcurrentDictionary<Identity, CancellationTokenSource> _tasks;
        private readonly Identifier _identifier;
        private readonly ILogger _logger;

        public event EventHandler<Identity> Done;
        public event EventHandler<Identity> Failed;

        public Manager(ILogger<Manager> logger)
        {
            _logger = logger;
            _identifier = new Identifier();
            _tasks = new ConcurrentDictionary<Identity, CancellationTokenSource>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal Task<Identity> StartAsync(Action action, CancellationTokenSource token = null)
        {
            var id = _identifier.GetNext(e => e.Start);
            var cancellationTokenSource = token ?? new CancellationTokenSource();
            using (_logger.BeginScope(id))
            {
                _logger.LogDebug("Starting new task...");
                var task = Task.Factory.StartNew(action,
                    cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning
                    | TaskCreationOptions.PreferFairness
                    | TaskCreationOptions.RunContinuationsAsynchronously,
                    TaskScheduler.Default);

                WrapAsync(id, task);
                if (_tasks.TryAdd(id, cancellationTokenSource))
                {
                    return Task.FromResult(id);
                }
                else
                {
                    throw new StartTaskException();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Task StopAsync(Identity id)
        {
            using (_logger.BeginScope(id))
            {
                _logger.LogDebug("Stopping the task...");
                if (_tasks.TryRemove(id, out var token))
                {
                    token.Cancel();
                }
                else
                {
                    _logger.LogWarning("Can't obtaining the task from list. Ignoring the action...");
                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task WrapAsync(Identity id, Task task)
        {
            using (_logger.BeginScope(id))
            {
                try
                {
                    await task;
                    _logger.LogDebug("Task was been completed.");
                    Done?.Invoke(this, id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Task was been failed.");
                    Failed?.Invoke(this, id);
                }
                finally
                {
                    if (!_tasks.TryRemove(id, out _))
                    {
                        _logger.LogWarning("Can't remove task's link from storage.");
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            foreach (var task in _tasks)
            {
                if (!task.Value.IsCancellationRequested)
                {
                    task.Value.Cancel();
                }
            }
        }
    }
}