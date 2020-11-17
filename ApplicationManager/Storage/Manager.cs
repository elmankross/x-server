using ApplicationManager.Storage.Exceptions;
using ApplicationManager.Storage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationManager.Storage
{
    public class Manager
    {
        private readonly ILogger _logger;
        private readonly Locker _locker;
        private readonly Configuration _configuration;
        private readonly Downloader.Manager _downloader;
        private readonly Tasker.Manager _tasker;
        private readonly Executor.Manager _executor;
        private readonly ConcurrentDictionary<string, Guid> _applicationTasks;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="downloader"></param>
        public Manager(
            IOptions<Configuration> configuration,
            ILogger<Manager> logger,
            Downloader.Manager downloader,
            Executor.Manager executor,
            Tasker.Manager tasker)
        {
            _configuration = configuration.Value;
            _locker = new Locker(_configuration.BaseDirectoryInfo);
            _logger = logger;
            _executor = executor;
            _downloader = downloader;
            _applicationTasks = new ConcurrentDictionary<string, Guid>();
            _tasker = tasker;
            _tasker.Done += Tasker_Done;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="e"></param>
        private void Tasker_Done(object _, Guid e)
        {
            foreach (var appTask in _applicationTasks)
            {
                if (appTask.Value == e)
                {
                    if (!_applicationTasks.TryRemove(appTask.Key, out var _))
                    {
                        _logger.LogWarning("Can't obtaining task's information from concurrent dictionary. Ignoring it...");
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationInfo> GetApplications()
        {
            var all = _downloader.Applications;
            var local = _locker.Applications;
            var query = from a in all
                        join l in local on a.Name equals l.Key into g
                        from set in g.DefaultIfEmpty()
                        select new ApplicationInfo(a)
                        {
                            ExecutionState = _applicationTasks.ContainsKey(a.Name)
                            ? ExecutionState.Executing
                            : ExecutionState.NotExecuted,
                            InstallationState = g.SingleOrDefault().Value?.InstallationState ?? InstallationState.NotInstalled
                        };
            return query.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        public async Task InstallAsync(string applicationName)
        {
            var taskId = await _tasker.StartAsync(() => InstallTaskAsync(applicationName));
            using (_logger.BeginScope(new { taskId }))
            {
                _logger.LogDebug("Locking installing app...");
                await _locker.LockAsync(taskId, _downloader.GetAppByName(applicationName), InstallationState.Installing);
                _logger.LogDebug("Locked!");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        private async Task InstallTaskAsync(string applicationName)
        {
            var id = Guid.NewGuid();
            using (_logger.BeginScope(new { applicationName, id }))
            {
                _logger.LogDebug("Downloading files...");

                var baseDir = _configuration.BaseDirectoryInfo;
                var app = await _downloader.DownloadAsync(applicationName);
                var tempName = id.ToString("N") + ".tmp";
                var tempFullName = Path.Combine(baseDir.FullName, tempName);
                using (var temp = File.OpenWrite(tempFullName))
                {
                    await app.Stream.CopyToAsync(temp);
                    await app.Stream.DisposeAsync();
                    await temp.FlushAsync();
                }

                using (var temp = File.OpenRead(tempFullName))
                {
                    if (!app.Hasher.Validate(temp))
                    {
                        throw new InvalidApplicationSignatureException("Application has invalid signature.");
                    }

                    var appDir = baseDir.CreateSubdirectory(applicationName);
                    foreach (var file in app.Unboxer.Unbox(temp))
                    {
                        using (_logger.BeginScope(file.FullName))
                        {
                            _logger.LogTrace("Saving the file...");

                            var path = Path.Combine(appDir.FullName, file.FullName);
                            var fileDirectory = new FileInfo(path).Directory;
                            if (!fileDirectory.Exists)
                            {
                                _logger.LogDebug("File's directory is not exists. Creating it...");
                                fileDirectory.Create();
                                _logger.LogDebug("Directory was been created.");
                            }

                            using (var stream = File.Create(path))
                            {
                                await file.Stream.CopyToAsync(stream);
                                await file.Stream.DisposeAsync();
                            }

                            _logger.LogTrace("Saved!");
                        }
                    }

                    _logger.LogDebug("Locking installed app...");
                    await _locker.LockAsync(id, _downloader.GetAppByName(applicationName), InstallationState.Installed);
                    _logger.LogDebug("Locked!");

                    _logger.LogDebug("Deleting temp file...");
                    File.Delete(tempFullName);
                    _logger.LogDebug("Done!");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public async Task UninstallAsync(string applicationName)
        {
            var baseDirectoryPath = _configuration.BaseDirectoryInfo.FullName;
            var path = Path.Combine(baseDirectoryPath, applicationName);
            using (_logger.BeginScope(path))
            {
                _logger.LogDebug("Deleting the directory...");

                await _locker.DeleteAsync(_downloader.GetAppByName(applicationName));
                Directory.Delete(path, true);

                _logger.LogDebug("Done!");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public async Task RunAsync(string applicationName)
        {
            var application = _downloader.GetAppByName(applicationName);
            application.BaseDirectory = Path.Combine(_configuration.BaseDirectoryInfo.FullName, applicationName);
            var id = await _executor.ExecuteAsync(application);
            _applicationTasks.TryAdd(applicationName, id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicatioName"></param>
        /// <returns></returns>
        public async Task TerminateAsync(string applicatioName)
        {
            if (_applicationTasks.TryGetValue(applicatioName, out var taskId))
            {
                await _executor.TerminateAsync(taskId);
            }
        }
    }
}
