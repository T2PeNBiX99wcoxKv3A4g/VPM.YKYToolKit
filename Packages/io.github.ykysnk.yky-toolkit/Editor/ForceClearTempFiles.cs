using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using UnityEditor;
using UnityEngine;
using Progress = UnityEditor.Progress;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class ForceClearTempFiles
    {
        private static readonly List<string> ClearFolders = new()
        {
            "../Packages/com.vrcfury.temp/Builds",
            "../Packages/nadena.dev.ndmf/__Generated",
            "ZZZ_GeneratedAssets"
        };

        [MenuItem("Tools/YKYToolkit/Force Clear Temp Files")]
        private static void Clear() => ClearAsync(CancellationToken.None).Forget();

        private static async UniTask ClearAsync(CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start("Force Clearing temp files...", "Preparing...", Progress.Options.Managed);

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(ForceClearTempFiles), "Cancel requested by the user.");
                cts.Cancel();
                return true;
            });

            var result = await Try.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var path in ClearFolders)
                {
                    var full = Path.GetFullPath(Path.Combine(Application.dataPath, path));
                    var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                    var trimmed = full.Replace(projectRoot + Path.DirectorySeparatorChar, "");
                    if (!Directory.Exists(trimmed)) continue;
                    var dirs = Directory.GetDirectories(trimmed);
                    var count = 0;

                    foreach (var dir in dirs)
                    {
                        if (cts.IsCancellationRequested)
                            throw new OperationCanceledException(cts.Token);
                        Directory.Delete(dir, true);
                        Progress.Report(progressId, (float)count / dirs.Length, $"Deleting: {dir}");
                        Utils.Log(nameof(ForceClearTempFiles), $"Deleted folder: {dir}");
                        count++;
                        if (stopwatch.ElapsedMilliseconds <= 30) continue;
                        await UniTask.Yield(cts.Token);
                        stopwatch.Restart();
                    }
                }

                AssetDatabase.Refresh();
                Progress.Finish(progressId);
            });

            result.OnFailure(ex =>
            {
                if (ex is OperationCanceledException)
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    Utils.LogWarning(nameof(ForceClearTempFiles), "Installation cancelled.");
                }
                else
                {
                    Progress.Finish(progressId, Progress.Status.Failed);
                    Utils.LogError(nameof(ForceClearTempFiles), $"Delete Error: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }
    }
}