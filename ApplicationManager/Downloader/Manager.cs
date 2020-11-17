using ApplicationManager.Downloader.Exceptions;
using ApplicationManager.Downloader.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ApplicationManager.Downloader
{
    public class Manager
    {
        public IEnumerable<ApplicationInfo> Applications => _applications;

        private readonly Applications _applications;
        private readonly ILogger _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applications"></param>
        public Manager(IOptions<Applications> applications, ILogger<Manager> logger)
        {
            _applications = applications.Value;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IApplication GetAppByName(string name) => _applications.Dictionary[name];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public async Task<ApplicationFile> DownloadAsync(string applicationName)
        {
            using (_logger.BeginScope(applicationName))
            {
                if (!_applications.Dictionary.ContainsKey(applicationName))
                {
                    throw new UnknownApplicationException(applicationName, "Application is not configured.");
                }

                var application = _applications.Dictionary[applicationName];
                var request = WebRequest.CreateHttp(application.Url);

                _logger.LogDebug("Calling the resource to download the file...");

                var response = await request.GetResponseAsync();

                return new ApplicationFile(
                    response.GetResponseStream(),
                    Unboxer.Manager.Get(new ContentType(response.ContentType)),
                    Hasher.Manager.Get(application.Hash));
            }
        }
    }
}