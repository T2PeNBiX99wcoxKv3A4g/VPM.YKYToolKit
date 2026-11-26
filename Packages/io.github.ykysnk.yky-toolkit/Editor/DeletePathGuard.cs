using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Decides whether an asset path points at a directory that must never be deleted. Assets and Packages are
    ///     always protected; the preferences page adds more directories on top.
    /// </summary>
    internal static class DeletePathGuard
    {
        private const string ExtraKey = "YKYToolkit/DeleteExtraProtectedDirectories";
        private static readonly string[] AlwaysProtected = { "Assets", "Packages" };

        private static readonly string ProjectRoot =
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        internal static IReadOnlyList<string> ExtraProtectedDirectories
        {
            get
            {
                var raw = EditorPrefs.GetString(ExtraKey, "");
                return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Normalize)
                    .Where(path => path.Length > 0 && !IsAlwaysProtected(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            set
            {
                var normalized = value.Select(Normalize)
                    .Where(path => path.Length > 0 && !IsAlwaysProtected(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                EditorPrefs.SetString(ExtraKey, string.Join("\n", normalized));
            }
        }

        internal static void ResetExtras() => EditorPrefs.DeleteKey(ExtraKey);

        /// <summary>True when this path is protected, or is a parent of a protected directory.</summary>
        internal static bool IsRestricted(string path)
        {
            var normalized = Normalize(path);
            if (normalized.Length == 0 || normalized == ".") return true;

            return AllProtected().Any(protectedPath =>
                normalized.Equals(protectedPath, StringComparison.OrdinalIgnoreCase) ||
                protectedPath.StartsWith(normalized + "/", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Project-relative, forward-slash path. "." and ".." are resolved, and paths outside the project return
        ///     an empty string so they cannot be used to slip past the check.
        /// </summary>
        internal static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            try
            {
                var full = Path.GetFullPath(Path.Combine(ProjectRoot, path.Trim().Replace('\\', '/')));
                var relative = Path.GetRelativePath(ProjectRoot, full);

                if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative))
                    return "";

                return relative.Replace('\\', '/').TrimEnd('/');
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static IEnumerable<string> AllProtected() => AlwaysProtected.Concat(ExtraProtectedDirectories);

        private static bool IsAlwaysProtected(string path) =>
            AlwaysProtected.Any(entry => string.Equals(entry, path, StringComparison.OrdinalIgnoreCase));
    }
}
