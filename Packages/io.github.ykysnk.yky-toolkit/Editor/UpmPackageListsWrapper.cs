using System;
using System.Collections.Generic;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    public class UpmPackageListsWrapper
    {
        public List<UpmPackageListWrapper> wrapPackageLists = new();
    }
}