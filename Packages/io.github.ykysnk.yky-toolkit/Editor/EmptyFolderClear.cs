using System.Collections.Generic;
using System.IO;
using System.Linq;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class EmptyFolderClear
    {
        private const string Title = "Clear Empty Folder";

        [MenuItem("Tools/YKYToolkit/Clear Empty Folder")]
        private static void Clear()
        {
            var reportPaths = GetEmptyFolders();

            if (reportPaths.Count < 1)
            {
                EditorUtility.DisplayDialog(Title, "No empty folders found.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(Title, $"Found {reportPaths.Count} empty folders.", "Start Clearing",
                    "Cancel")) return;

            while (reportPaths.Count > 0)
            {
                var count = 0;
                reportPaths.ForEach(path =>
                {
                    var fullPath = Path.GetFullPath(path);
                    var cutPath = fullPath.LastPath("Assets\\") ?? fullPath.LastPath("Assets/") ?? "";
                    EditorUtility.DisplayProgressBar(Title, cutPath, (float)count / reportPaths.Count);
                    AssetDatabase.DeleteAsset(path);
                    count++;
                });
                reportPaths = GetEmptyFolders();
            }

            EditorUtility.ClearProgressBar();
        }

        private static List<string> GetEmptyFolders()
        {
            return Directory.GetDirectories("Assets/", "*", SearchOption.AllDirectories).Where(path =>
                Directory.GetFiles(path).Length < 1 && Directory.GetDirectories(path).Length < 1).ToList();
        }
    }
}