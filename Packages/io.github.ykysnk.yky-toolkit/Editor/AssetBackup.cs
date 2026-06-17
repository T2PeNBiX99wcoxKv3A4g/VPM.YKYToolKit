using System;
using System.Diagnostics;
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
            var stopwatch = Stopwatch.StartNew();

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

                if (stopwatch.ElapsedMilliseconds <= 30) continue;
                await UniTask.Yield();
                stopwatch.Restart();
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