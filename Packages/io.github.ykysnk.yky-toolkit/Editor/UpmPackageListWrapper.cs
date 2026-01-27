using System;
using System.Collections.Generic;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    public class UpmPackageListWrapper
    {
        public string wrapName = "";
        public List<UpmInstallerPackage> wrapPackages = new();
    }
}