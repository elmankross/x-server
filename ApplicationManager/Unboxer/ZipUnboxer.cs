using ApplicationManager.Unboxer.Models;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ApplicationManager.Unboxer
{
    public class ZipUnboxer : Manager
    {
        internal override IEnumerable<UnboxedFileInfo> Unbox(Stream stream)
        {
            var root = string.Empty;
            var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
            for (var i = 0; i < archive.Entries.Count; i++)
            {
                var entry = archive.Entries[i];

                if (string.IsNullOrEmpty(entry.Name))
                {
                    // remove root directory
                    if (i == 0)
                    {
                        var nextEntry = archive.Entries[i + 1];
                        if (entry.FullName.Split('/').Length <= nextEntry.FullName.Split('/').Length)
                        {
                            root = entry.FullName;

                        }
                    }
                    continue;
                }

                var fullName = entry.FullName;
                if (root != string.Empty)
                {
                    fullName = fullName.Replace(root, null);
                }

                yield return new UnboxedFileInfo(fullName, entry.Open());
            }
            archive.Dispose();
        }
    }
}
