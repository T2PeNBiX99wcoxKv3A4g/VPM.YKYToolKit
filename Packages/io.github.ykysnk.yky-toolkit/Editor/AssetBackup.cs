using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class AssetBackup
    {
        [MenuItem("Assets/YKYToolkit/Asset Backup #b")]
        private static void Backup()
        {
            if (Selection.assetGUIDs.Length < 1) return;

            var now = DateTime.Now;
            var date = $"{now:yy-MM-dd}";

            BackupAsync(date, Selection.assetGUIDs).Forget();
        }

        private static async UniTask BackupAsync(string date, string[] guids)
        {
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var pathDir = Path.GetDirectoryName(path);
                var ext = Path.GetExtension(path);
                var newName = $"{Path.GetFileNameWithoutExtension(path)}_{date}";
                var newPath = $"{pathDir}/{newName}{ext}";

                AssetDatabase.CopyAsset(path, string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(newPath))
                    ? newPath
                    : GetNewPathUntilNotExist(pathDir, newName, ext));

                await UniTask.NextFrame();
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
}