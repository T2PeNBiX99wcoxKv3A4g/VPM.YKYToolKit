using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
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
        private static void Clear()
        {
            _ = ClearFolder(CancellationToken.None);
        }

        private static async UniTask ClearFolder(CancellationToken token)
        {
            if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start(Title, "Clearing empty folders...");

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
                    // TODO: Replace to custom dialog
                    EditorUtility.DisplayDialog(Title, "No empty folders found.", "OK");
                    return;
                }

                if (!EditorUtility.DisplayDialog(Title, $"Found {reportPaths.Count} empty folders.", "Start Clearing",
                        "Cancel")) return;

                while (reportPaths.Count > 0)
                {
                    var count = 0;
                    foreach (var path in reportPaths)
                    {
                        var fullPath = Path.GetFullPath(path);
                        var cutPath = fullPath.LastPath("Assets\\") ?? fullPath.LastPath("Assets/") ?? "";
                        Progress.Report(progressId, (float)count / reportPaths.Count, $"Deleting: {cutPath}");
                        AssetDatabase.DeleteAsset(path);
                        count++;

                        await UniTask.Yield();
                    }

                    reportPaths = await GetEmptyFolders();
                }

                Progress.Finish(progressId);
            }
            catch (OperationCanceledException)
            {
                Progress.Finish(progressId, Progress.Status.Canceled);
                Progress.UnregisterCancelCallback(progressId);
                Utils.LogWarning(nameof(EmptyFolderClear), "Installation cancelled.");
            }
            catch (Exception ex)
            {
                Progress.Finish(progressId, Progress.Status.Failed);
                Progress.UnregisterCancelCallback(progressId);
                Utils.LogError(nameof(EmptyFolderClear), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Interlocked.Exchange(ref _isWorking, 0);
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