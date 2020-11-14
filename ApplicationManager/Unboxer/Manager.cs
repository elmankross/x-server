using ApplicationManager.Unboxer.Exceptions;
using ApplicationManager.Unboxer.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;

namespace ApplicationManager.Unboxer
{
    public abstract class Manager
    {
        /// <summary>
        /// 
        /// </summary>
        protected Manager()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Manager Get(ContentType type)
        {
            return type.MediaType switch
            {
                "application/zip" => new ZipUnboxer(),
                _ => throw new InvalidUnboxerTypeException(type.MediaType, "Unrecognized unboxer type. It's not implemented yet.")
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal abstract IEnumerable<UnboxedFileInfo> Unbox(Stream stream);
    }
}
