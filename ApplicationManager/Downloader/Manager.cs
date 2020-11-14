using ApplicationManager.Downloader.Exceptions;
using ApplicationManager.Downloader.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;

namespace ApplicationManager.Downloader
{
    public class Manager
    {
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
        /// Get names of all exist applications
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAvailableApps()
        {
            return _applications.Keys;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<IFileInfo> DownloadAsync(string applicationName)
        {
            using (_logger.BeginScope(applicationName))
            {
                if (!_applications.ContainsKey(applicationName))
                {
                    throw new UnknownApplicationException(applicationName, "Application is not configured.");
                }

                var application = _applications[applicationName];

                _logger.LogDebug("Looking for hasher...");

                var hasher = Hasher.Manager.Get(application.Hash);

                _logger.LogDebug("Hasher was been found.");

                var request = WebRequest.CreateHttp(application.Url);

                _logger.LogDebug("Calling the resource to download the file...");

                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                var stream = new MemoryStream();
                await responseStream.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                _logger.LogDebug("Resource was replied. File was been downloaded.");
                _logger.LogDebug("Validating received file...");

                if (!hasher.Validate(stream))
                {
                    throw new InvalidApplicationSignatureException("Downloaded application has invalid signature.");
                }

                _logger.LogDebug("Validation was been passed.");
                _logger.LogDebug("Looking for unboxer...");

                var unboxer = Unboxer.Manager.Get(new ContentType(response.ContentType));

                _logger.LogDebug("Unboxer was been found. Trying to unbox...");

                var unboxed = unboxer.Unbox(stream);

                foreach (var file in unboxed)
                {
                    yield return file;
                }

                _logger.LogDebug("Done!");

                response.Dispose();
                await stream.DisposeAsync();
                await responseStream.DisposeAsync();
            }
        }
    }
}