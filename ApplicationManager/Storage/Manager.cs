using ApplicationManager.Identifier.Models;
using ApplicationManager.Storage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly Identifier _identifier;
        private readonly ApplicationTasks _applicationTasks;
        private readonly StorageEnv _env;


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
            Tasker.Manager<Manager> tasker)
        {
            _configuration = configuration.Value;
            _env = new StorageEnv()
            {
                [k => k.BaseDirectory] = _configuration.BaseDirectoryInfo.FullName
            };
            _locker = new Locker(_configuration.BaseDirectoryInfo);
            _logger = logger;
            _executor = executor;
            _executor.Exited += Executor_Exited;
            _downloader = downloader;
            _applicationTasks = new ApplicationTasks();
            _identifier = new Identifier();
            _tasker = tasker;
            _tasker.Done += OnTasker_Done;
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
                            WebUrl = a.WebUrl,
                            Dependencies = a.Dependencies,
                            ExecutionState = GetExecutionState(a.Name),
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
            using (_logger.BeginScope(new { applicationName }))
            {
                var id = await _tasker.StartAsync(() => InstallTaskAsync(applicationName));
                using (_logger.BeginScope(id))
                {
                    _logger.LogDebug("Locking installing app...");
                    await _locker.LockAsync(id, _downloader.GetAppByName(applicationName), InstallationState.Installing);
                    _logger.LogDebug("Locked!");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ExecutionState GetExecutionState(string appName)
        {
            if (_applicationTasks.ContainsKey((appName, Executor.Identifier.Events.ExecuteMain))
             || _applicationTasks.ContainsKey((appName, Executor.Identifier.Events.ExecuteHookBefore)))
            {
                return ExecutionState.Executing;
            }

            if (_applicationTasks.ContainsKey((appName, Executor.Identifier.Events.Terminate)))
            {
                return ExecutionState.Terminating;
            }

            return ExecutionState.NotExecuted;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        private async Task InstallTaskAsync(string applicationName)
        {
            var id = _identifier.GetNext(e => e.Install);
            using (_logger.BeginScope(new { applicationName, id }))
            {
                _logger.LogDebug("Downloading files...");

                var app = await _downloader.DownloadAsync(applicationName);
                var tempName = id.Number.ToString("N") + ".tmp";
                var tempFullName = Path.Combine(_env[x => x.BaseDirectory], tempName);
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
                        _logger.LogError("Application has invalid signature.");
                    }

                    var appDir = _configuration.BaseDirectoryInfo.CreateSubdirectory(applicationName);
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
            using (_logger.BeginScope(new { path }))
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
            using (_logger.BeginScope(new { applicationName }))
            {
                var application = _downloader.GetAppByName(applicationName);
                var id = await _executor.ExecuteAsync(_env, application);
                id.NewChildren += OnId_NewChild;
                if (!_applicationTasks.TryAdd((applicationName, id.Type), id))
                {
                    _logger.LogWarning("Can't add event id to internal storage. Ignoring it...");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicatioName"></param>
        /// <returns></returns>
        public async Task TerminateAsync(string applicatioName)
        {
            using (_logger.BeginScope(new { applicatioName }))
            {
                var key = (applicatioName, Executor.Identifier.Events.ExecuteMain);
                if (_applicationTasks.TryRemove(key, out var id))
                {
                    var eventId = await _tasker.StartAsync(() => _executor.TerminateAsync(id));
                    if (!_applicationTasks.TryAdd((applicatioName, Executor.Identifier.Events.Terminate), eventId))
                    {
                        _logger.LogWarning("Can't add new event information to the storage. Ignoring it...");
                    }
                }
                else
                {
                    _logger.LogWarning("There is no information about terminating application. Ignoring it...");
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childId"></param>
        private void OnId_NewChild(object parent, Identity childId)
        {
            if (childId.Type == Executor.Identifier.Events.ExecuteMain)
            {
                if (parent is Identity parentId)
                {
                    if (!_applicationTasks.TryGetKey(parentId, out var key))
                    {
                        _logger.LogWarning("Can't obtaining task's information from application tasks. Ignoring it...");
                    }

                    if (!_applicationTasks.TryAdd((key.Item1, childId.Type), childId))
                    {
                        _logger.LogWarning("Can't add task's information to application tasks. Ignoring it...");
                    }

                    _applicationTasks.TryRemove(key, out var _);
                    parentId.NewChildren -= OnId_NewChild;
                }
           }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="id"></param>
        private void OnTasker_Done(object _, Identity id)
        {
            if (_applicationTasks.TryGetKey(id, out var key))
            {
                if (!_applicationTasks.TryRemove((key.Item1, key.Item2), out var _))
                {
                    _logger.LogDebug("Information about running application was been removed.");
                }
            }
            else
            {
                _logger.LogDebug("Can't obtaining task's information from application tasks. Ignoring it...");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Executor_Exited(object sender, Identity e)
        {
            if (e.Type != Executor.Identifier.Events.ExecuteMain)
            {
                _logger.LogDebug("Not interested event type. Skip it...");
                return;
            }

            if (!_applicationTasks.TryGetKey(e, out var key))
            {
                _logger.LogWarning("Cannot find the task to remove from the list. Ignoring it...");
            }

            if (!_applicationTasks.TryRemove(key, out var _))
            {
                _logger.LogWarning("Cannot remove the task from the list. Ignoring it...");
            }
        }
    }
}