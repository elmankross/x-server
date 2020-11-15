using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ApplicationManager.Storage.Models
{
    /// <summary>
    /// 
    /// </summary>
    internal class ApplicationLockListBuffer : ConcurrentDictionary<string, ApplicationLock>
    {
        internal ApplicationLockListBuffer() { }
        internal ApplicationLockListBuffer(Dictionary<string, ApplicationLock> init)
            : base(init)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal ApplicationLockList GetLock()
        {
            return new ApplicationLockList(Values);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ApplicationLockList : List<ApplicationLock>
    {
        // for deserialization
        public ApplicationLockList() { }
        internal ApplicationLockList(ICollection<ApplicationLock> init)
        {
            AddRange(init);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal ApplicationLockListBuffer GetBuffer()
        {
            return new ApplicationLockListBuffer(this.ToDictionary(x => x.Name));
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ApplicationLock : ApplicationInfo
    {
        [JsonInclude]
        public Guid Id { get; private set; }
        [JsonInclude]
        public DateTime CreatedAt { get; private set; }

        [JsonConstructor]
        public ApplicationLock()
        {
        }

        internal ApplicationLock(Guid id, Downloader.Models.ApplicationInfo source)
            : base(source)
        {
            Id = id;
            CreatedAt = DateTime.Now;
        }
    }
}
