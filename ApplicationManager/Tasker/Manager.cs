using ApplicationManager.Tasker.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ApplicationManager.Tasker
{
    public class Manager
    {
        private readonly ConcurrentDictionary<Guid, Task> _tasks;
        private readonly ILogger _logger;

        public event EventHandler<Guid> Done;
        public event EventHandler<Guid> Failed;

        public Manager(ILogger<Manager> logger)
        {
            _logger = logger;
            _tasks = new ConcurrentDictionary<Guid, Task>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        internal Task<Guid> StartAsync(Task task)
        {
            var id = Guid.NewGuid();
            using (_logger.BeginScope(id))
            {
                _logger.LogDebug("Starting new task...");
                if (_tasks.TryAdd(id, WrapAsync(id, task)))
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
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task WrapAsync(Guid id, Task task)
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
    }
}