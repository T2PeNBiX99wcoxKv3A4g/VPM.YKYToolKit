using System;
using System.IO;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor;

internal static class AssetBackup
{
    private const string MenuPath = $"Assets/{Util.Name}/Asset Backup #b";

    [MenuItem(MenuPath, false)]
    private static void Backup()
    {
        if (Selection.assetGUIDs.Length < 1) return;

        var now = DateTime.Now;
        var date = $"{now:yy-MM-dd}";

        foreach (var guid in Selection.assetGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var pathDir = Path.GetDirectoryName(path);
            var ext = Path.GetExtension(path);
            var newName = $"{Path.GetFileNameWithoutExtension(path)}_{date}";
            var newPath = $"{pathDir}/{newName}{ext}";

            AssetDatabase.CopyAsset(path, string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(newPath))
                ? newPath
                : GetNewPathUntilNotExist(pathDir, newName, ext));
        }
    }

    private static string GetNewPathUntilNotExist(string? pathDir, string name, string? ext)
    {
        var count = 1;
        string newPath;

        while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(newPath = $"{pathDir}/{name}.{count:000}{ext}")))
            count++;

        return newPath;
    }
}