using ApplicationManager.Storage.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApplicationManager.Storage
{
    /// <summary>
    /// 
    /// </summary>
    public class Locker : IAsyncDisposable
    {
        private const string FILE_NAME = "applications.lock";
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = false,
            WriteIndented = true
        };
        private readonly FileStream _stream;
        internal ApplicationLockListBuffer Applications { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDirectory"></param>
        internal Locker(DirectoryInfo baseDirectory)
        {
            var file = new FileInfo(Path.Combine(baseDirectory.FullName, FILE_NAME));
            _stream = file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (_stream.Length != 0)
            {
                ReadAsync().Wait();
            }
            else
            {
                Applications = new ApplicationLockListBuffer();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal async Task LockAsync(Guid id, Downloader.Models.IDisplayable info, InstallationState state)
        {
            Applications.AddOrUpdate(info.Name, _ => new ApplicationLock(id, info)
            {
                InstallationState = state
            }, (_, item) => { item.InstallationState = state; return item; });

            await WriteAsync();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        internal async Task DeleteAsync(Downloader.Models.IDisplayable info)
        {
            if (Applications.TryRemove(info.Name, out var _))
            {
                await WriteAsync();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ReadAsync()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var @lock = await JsonSerializer.DeserializeAsync<ApplicationLockList>(_stream, _options);
            Applications = @lock.GetBuffer();
        }


        /// <summary>
        /// 
        /// </summary>
        private async Task WriteAsync()
        {
            var @lock = Applications.GetLock();
            _stream.SetLength(0);
            await JsonSerializer.SerializeAsync(_stream, @lock, _options);
            await _stream.FlushAsync();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await _stream.DisposeAsync();
        }
    }
}
