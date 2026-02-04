using System;
using System.Collections.Generic;
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
            "../Packages/nadena.dev.ndmf/__Generated"
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
                foreach (var clearFolder in ClearFolders)
                {
                    var path = Path.Combine(Application.dataPath, clearFolder);

                    if (!Directory.Exists(path)) continue;
                    var dirs = Directory.GetDirectories(path);
                    var count = 0;

                    foreach (var dir in dirs)
                    {
                        if (cts.IsCancellationRequested)
                            throw new OperationCanceledException(cts.Token);
                        Directory.Delete(dir, true);
                        Progress.Report(progressId, (float)count / dirs.Length, $"Deleting: {dir}");
                        Utils.Log(nameof(ForceClearTempFiles), $"Deleted folder: {dir}");
                        count++;
                        await UniTask.DelayFrame(10, cancellationToken: cts.Token);
                    }
                }

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