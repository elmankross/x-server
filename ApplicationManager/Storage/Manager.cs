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

        /// <summary>
        /// 
        /// </summary>
        private readonly Configuration _configuration;

        /// <summary>
        /// 
        /// </summary>
        private readonly Downloader.Manager _downloader;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="downloader"></param>
        public Manager(
            IOptions<Configuration> configuration,
            ILogger<Manager> logger,
            Downloader.Manager downloader)
        {
            _configuration = configuration.Value;
            _logger = logger;
            _downloader = downloader;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationInfo> GetApplications()
        {
            var subdirectories = _configuration.BaseDirectoryInfo.GetDirectories().ToDictionary(x => x.Name);
            foreach (var availableApp in _downloader.GetAvailableApps())
            {
                yield return new ApplicationInfo
                {
                    Name = availableApp,
                    Installed = subdirectories.ContainsKey(availableApp)
                };
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        public async Task InstallAsync(string applicationName)
        {
            using (_logger.BeginScope(applicationName))
            {
                _logger.LogDebug("Downloading files...");

                var files = _downloader.DownloadAsync(applicationName);
                var baseDirectory = _configuration.BaseDirectoryInfo.CreateSubdirectory(applicationName);
                await foreach (var file in files)
                {
                    using (_logger.BeginScope(file.FullName))
                    {
                        _logger.LogTrace("Saving the file...");

                        var path = Path.Combine(baseDirectory.FullName, file.FullName);
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
                        }

                        _logger.LogTrace("Saved!");
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public Task UninstallAsync(string applicationName)
        {
            var baseDirectoryPath = _configuration.BaseDirectoryInfo.FullName;
            var path = Path.Combine(baseDirectoryPath, applicationName);
            using (_logger.BeginScope(path))
            {
                _logger.LogDebug("Deleting the directory...");

                Directory.Delete(path, true);

                _logger.LogDebug("Done!");

                return Task.CompletedTask;
            }
        }
    }
}
