using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using Progress = UnityEditor.Progress;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class EmptyFolderClear
    {
        private const string Title = "Clear Empty Folder";
        private static int _isWorking;

        [MenuItem("Tools/YKYToolkit/Clear Empty Folder")]
        private static void Clear() => ClearFolder(CancellationToken.None).Forget();

        private static async UniTask ClearFolder(CancellationToken token)
        {
            if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start("Clearing empty folders...", "Preparing...", Progress.Options.Sticky);

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(EmptyFolderClear), "Cancel requested by user.");
                cts.Cancel();
                return true;
            });

            try
            {
                var reportPaths = await GetEmptyFolders();
                if (reportPaths.Count < 1)
                {
                    _ = EditorUtils.DisplayDialogAsync(Title, "No empty folders found.");
                    Progress.Report(progressId, 1, "No empty folders found.");
                    Progress.Finish(progressId);
                    return;
                }

                if (!await EditorUtils.DisplayDialogAsync(Title, $"Found {reportPaths.Count} empty folders.",
                        "Start Clearing", "Cancel")) return;

                while (reportPaths.Count > 0)
                {
                    var count = 0;
                    foreach (var path in reportPaths)
                    {
                        var fullPath = Path.GetFullPath(path);
                        var cutPath = fullPath.LastPath("Assets\\") ?? fullPath.LastPath("Assets/") ?? "";
                        if (cts.IsCancellationRequested)
                            throw new OperationCanceledException(cts.Token);
                        Progress.Report(progressId, (float)count / reportPaths.Count, $"Deleting: {cutPath}");
                        AssetDatabase.DeleteAsset(path);
                        count++;
                        await UniTask.NextFrame(cts.Token);
                    }

                    reportPaths = await GetEmptyFolders();
                }

                Progress.Finish(progressId);
            }
            catch (OperationCanceledException)
            {
                Progress.Finish(progressId, Progress.Status.Canceled);
                Utils.LogWarning(nameof(EmptyFolderClear), "Installation cancelled.");
            }
            catch (Exception ex)
            {
                Progress.Finish(progressId, Progress.Status.Failed);
                Utils.LogError(nameof(EmptyFolderClear), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Interlocked.Exchange(ref _isWorking, 0);
                Progress.UnregisterCancelCallback(progressId);
            }
        }

        private static async UniTask<List<string>> GetEmptyFolders()
        {
            return await UniTask.RunOnThreadPool(() => Directory
                .GetDirectories("Assets/", "*", SearchOption.AllDirectories).Where(path =>
                    Directory.GetFiles(path).Length < 1 && Directory.GetDirectories(path).Length < 1).ToList());
        }
    }
}