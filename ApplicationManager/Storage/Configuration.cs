using ApplicationManager.Storage.Exceptions;
using System.IO;

namespace ApplicationManager.Storage
{
    public class Configuration
    {
        /// <summary>
        /// 
        /// </summary>
        public string BaseDirectory
        {
            get => BaseDirectoryInfo?.Name;
            set
            {
                var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), value));
                if (directory.Exists)
                {
                    if (directory.Attributes == FileAttributes.ReadOnly)
                    {
                        throw new IncompatibleDirectoryPermissionException(value, "Directory is in readonly mode.");
                    }

                    BaseDirectoryInfo = directory;
                }
                else
                {
                    throw new DirectoryNotExistsException(value, "You should create directory manualy before use it in configuration.");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        internal DirectoryInfo BaseDirectoryInfo { get; private set; }
    }
}
