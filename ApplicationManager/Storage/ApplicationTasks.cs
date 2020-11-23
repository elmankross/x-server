using ApplicationManager.Identifier.Models;
using System.Collections.Concurrent;
using System.Linq;

namespace ApplicationManager.Storage
{
    /// <summary>
    /// Stores all applications-events related event's id
    /// </summary>
    internal class ApplicationTasks : ConcurrentDictionary<(string, string), Identity>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool TryGetKey(Identity id, out (string, string) key)
        {
            if (!Values.Contains(id))
            {
                key = (string.Empty, string.Empty);
                return false;
            }

            var found = this.SingleOrDefault(x => x.Value == id);

            key = found.Key;
            return true;
        }
    }
}
