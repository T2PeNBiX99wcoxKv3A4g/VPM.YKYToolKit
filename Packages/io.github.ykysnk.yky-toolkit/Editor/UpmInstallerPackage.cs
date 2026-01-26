using System;
using System.Diagnostics.CodeAnalysis;
using io.github.ykysnk.utils.Extensions;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    public class UpmInstallerPackage : IEquatable<UpmInstallerPackage>
    {
        public string packageName;
        public string version;

        public UpmInstallerPackage(string packageName, string? version)
        {
            this.packageName = packageName;
            this.version = version ?? "";
        }

        public UpmInstallerPackage(string fullName)
        {
            var split = fullName.Split('@');
            packageName = split[0];
            version = split.GetValueOrDefault(1) ?? "";
        }

        public string FullName => string.IsNullOrEmpty(version) ? packageName : $"{packageName}@{version}";

        public bool Equals(UpmInstallerPackage? other) => other is not null &&
                                                          string.Equals(packageName, other.packageName,
                                                              StringComparison.Ordinal) && string.Equals(version,
                                                              other.version, StringComparison.Ordinal);

        public override string ToString() => FullName;

        public static implicit operator UpmInstallerPackage(string fullName) => new(fullName);
        public static implicit operator string(UpmInstallerPackage package) => package.FullName;

        public override bool Equals(object? obj) => Equals(obj as UpmInstallerPackage);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() => HashCode.Combine(packageName, version);
    }
}