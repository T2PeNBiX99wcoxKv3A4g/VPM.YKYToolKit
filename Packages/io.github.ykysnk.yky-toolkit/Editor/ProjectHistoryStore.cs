using System;
using System.IO;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Small JSON files under <c>UserSettings/YKYToolkit</c>. These live inside the project, so a history stays
    ///     with one project instead of EditorPrefs, which is shared by every Unity project on the machine.
    /// </summary>
    internal static class ProjectHistoryStore
    {
        private static string DirectoryPath
        {
            get
            {
                var project = Directory.GetParent(Application.dataPath);
                return project == null ? "" : Path.Combine(project.FullName, "UserSettings", "YKYToolkit");
            }
        }

        internal static string Read(string fileName)
        {
            var directory = DirectoryPath;
            if (string.IsNullOrEmpty(directory)) return string.Empty;

            try
            {
                var path = Path.Combine(directory, fileName);
                return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[YKYToolkit] Failed to read {fileName}: {ex.Message}");
                return string.Empty;
            }
        }

        internal static void Write(string fileName, string json)
        {
            var directory = DirectoryPath;
            if (string.IsNullOrEmpty(directory)) return;

            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, fileName), json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[YKYToolkit] Failed to write {fileName}: {ex.Message}");
            }
        }
    }
}
