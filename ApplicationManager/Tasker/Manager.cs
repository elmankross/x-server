using ApplicationManager.Tasker.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationManager.Tasker
{
    public class Manager : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _tasks;
        private readonly ILogger _logger;

        public event EventHandler<Guid> Done;
        public event EventHandler<Guid> Failed;

        public Manager(ILogger<Manager> logger, IApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            applicationLifetime.ApplicationStopping.Register(Dispose);
            _tasks = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal Task<Guid> StartAsync(Action action, CancellationTokenSource token = null)
        {
            var taskId = Guid.NewGuid();
            var cancellationTokenSource = token ?? new CancellationTokenSource();
            using (_logger.BeginScope(new { taskId }))
            {
                _logger.LogDebug("Starting new task...");
                var task = Task.Factory.StartNew(action,
                    cancellationTokenSource.Token,
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);

                WrapAsync(taskId, task);
                if (_tasks.TryAdd(taskId, cancellationTokenSource))
                {
                    return Task.FromResult(taskId);
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
        /// <param name="taskId"></param>
        /// <returns></returns>
        internal Task StopAsync(Guid taskId)
        {
            using (_logger.BeginScope(new { taskId }))
            {
                _logger.LogDebug("Stopping the task...");
                if (_tasks.TryRemove(taskId, out var token))
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
        /// <param name="taskId"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task WrapAsync(Guid taskId, Task task)
        {
            using (_logger.BeginScope(new { taskId }))
            {
                try
                {
                    await task;
                    _logger.LogDebug("Task was been completed.");
                    Done?.Invoke(this, taskId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Task was been failed.");
                    Failed?.Invoke(this, taskId);
                }
                finally
                {
                    if (!_tasks.TryRemove(taskId, out _))
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