using ApplicationManager.Identifier.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationManager.Storage
{
    /// <summary>
    /// Stores all applications related event's id
    /// </summary>
    internal class ApplicationTasks : ConcurrentDictionary<string, HashSet<KeyValuePair<Identity, string>>>
    {
        internal Dictionary<Identity, string> TaskApplications { get; private set; } = new Dictionary<Identity, string>(0);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrInsert(string key, Func<Identity> value)
        {
            base.AddOrUpdate(key,
                (_) =>
                {
                    return new HashSet<KeyValuePair<Identity, string>>
                    {
                        new KeyValuePair<Identity, string>(value(), key)
                    };
                },
                (_, set) =>
                {
                    set.Add(new KeyValuePair<Identity, string>(value(), key));
                    return set;
                });

            TaskApplications = Values
                .SelectMany(x => x.ToDictionary(y => y.Key, y => y.Value))
                .ToDictionary(x => x.Key, x => x.Value);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="type">Event type</param>
        /// <returns></returns>
        public bool TryGetApplicationEvent(string application, string type, out Identity id)
        {
            if (!ContainsKey(application))
            {
                id = default;
                return false;
            }
            else
            {
                var @event = this[application].SingleOrDefault(x => x.Key.Type == type);
                id = @event.Key;
                return !string.IsNullOrEmpty(@event.Value);

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool TryRemoveApplicationEvent(string application, string type)
        {
            if (!ContainsKey(application))
            {
                return false;
            }

            this[application].RemoveWhere(x => x.Key.Type == type);

            TaskApplications = Values
                .SelectMany(x => x.ToDictionary(y => y.Key, y => y.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            return true;
        }
    }
}
