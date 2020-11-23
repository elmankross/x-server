using System;
using System.Collections.Generic;

namespace ApplicationManager.Storage.Models
{
    public class StorageEnv : Dictionary<string, string>
    {
        private static readonly Keys _keys = new Keys();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public string this[Func<Keys, string> selector]
        {
            get => this[selector(_keys)];
            set => this[selector(_keys)] = value;
        }


        public class Keys
        {
            public string BaseDirectory { get; } = ":BASEDIR";
        }
    }
}
