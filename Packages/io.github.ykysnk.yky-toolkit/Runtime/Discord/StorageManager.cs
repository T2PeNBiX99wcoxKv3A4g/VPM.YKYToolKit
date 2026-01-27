using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Discord
{
    [PublicAPI]
    public partial class StorageManager
    {
        public IEnumerable<FileStat> Files()
        {
            var fileCount = Count();
            var files = new List<FileStat>();
            for (var i = 0; i < fileCount; i++)
                files.Add(StatAt(i));

            return files;
        }
    }
}