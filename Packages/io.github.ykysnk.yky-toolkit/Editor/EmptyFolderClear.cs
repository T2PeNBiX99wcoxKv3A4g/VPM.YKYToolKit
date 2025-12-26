using System.IO;
using System.Linq;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor;

internal static class EmptyFolderClear
{
    private const string Title = "Clear Empty Folder";

    [MenuItem($"Tools/{Util.Name}/{Title}")]
    private static void Clear()
    {
        var reportPaths = Directory.GetDirectories("Assets/", "*", SearchOption.AllDirectories).Where(path =>
            Directory.GetFiles(path).Length < 1 && Directory.GetDirectories(path).Length < 1).ToList();

        if (reportPaths.Count < 1)
        {
            EditorUtility.DisplayDialog(Title, "No empty folders found.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog(Title, $"Found {reportPaths.Count} empty folders.", "Start Clearing",
                "Cancel")) return;

        var count = 0;
        reportPaths.ForEach(path =>
        {
            EditorUtility.DisplayProgressBar(Title, $"Clearing Empty Folder\n{path}", (float)count / reportPaths.Count);
            AssetDatabase.DeleteAsset(path);
            count++;
        });
        EditorUtility.ClearProgressBar();
    }
}