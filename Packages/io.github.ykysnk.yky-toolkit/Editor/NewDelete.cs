using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using UnityEditor;
using Object = UnityEngine.Object;
using Progress = UnityEditor.Progress;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class NewDelete
    {
        private const string ToolsMenuPath = "Tools/YKYToolkit/Warning Window for Delete Selected";
        private const string AssetsMenuPath = "Assets/YKYToolkit/Warning Window for Delete Selected";
        private const string EditorKey = "YKYToolkit/DeleteSelectedWarnWindow";

        internal static bool ShowWarnWindow
        {
            get => EditorPrefs.GetBool(EditorKey, true);
            set => EditorPrefs.SetBool(EditorKey, value);
        }

        [MenuItem(ToolsMenuPath, false, Util.Twe)]
        [MenuItem(AssetsMenuPath)]
        private static void DeleteSelectedShowMenu() => ShowWarnWindow = !ShowWarnWindow;

        [MenuItem(ToolsMenuPath, true, Util.Twe)]
        [MenuItem(AssetsMenuPath, true)]
        private static bool DeleteSelectedGameObjectsShowMenuValidate()
        {
            Menu.SetChecked(ToolsMenuPath, ShowWarnWindow);
            Menu.SetChecked(AssetsMenuPath, ShowWarnWindow);
            return true;
        }

        [MenuItem("GameObject/YKYToolkit/Delete Selected", false, Util.Twe)]
        [MenuItem("Assets/YKYToolkit/Delete Selected _DEL")]
        private static void DeleteSelected()
        {
            var guids = Selection.assetGUIDs;
            if (guids is { Length: > 0 })
            {
                DeleteSelectedAssetsAsync(guids, CancellationToken.None).Forget();
                return;
            }

            var selectedObjects = Selection.objects;
            if (selectedObjects is { Length: > 0 })
                DeleteSelectedObjectsAsync(selectedObjects, CancellationToken.None).Forget();
        }

        private static async UniTask DeleteSelectedObjectsAsync(Object[] selectedObjects, CancellationToken token)
        {
            var total = selectedObjects.Length;
            var current = 0;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start("Deleting Objects", "Delete Object...", Progress.Options.Managed);

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(DeleteSelectedObjectsAsync), "Cancel requested by user.");
                cts.Cancel();
                return true;
            });

            var result = await Try.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var selectedObject in selectedObjects)
                {
                    if (selectedObject == null) continue;
                    var name = selectedObject.name;

                    if (cts.IsCancellationRequested)
                        throw new OperationCanceledException(cts.Token);

                    Try.Run(() => Undo.DestroyObjectImmediate(selectedObject)).OnFailure(ex =>
                        Utils.LogError(nameof(DeleteSelectedObjectsAsync),
                            $"Failed to delete {name}: {ex.Message}\n{ex.StackTrace}"));

                    current++;
                    var progress = (float)current / total;
                    Progress.Report(progressId, progress, $"Deleted: {name}");
                    if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                    await UniTask.Yield(cts.Token);
                    stopwatch.Restart();
                }

                Progress.Finish(progressId);
            });

            result.OnFailure(ex =>
            {
                if (ex is OperationCanceledException)
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    Utils.LogWarning(nameof(DeleteSelectedObjectsAsync), "Delete cancelled.");
                }
                else
                {
                    Progress.Finish(progressId, Progress.Status.Failed);
                    Utils.LogError(nameof(DeleteSelectedObjectsAsync), $"Delete Error: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        private static async UniTask DeleteSelectedAssetsAsync(string[] guids, CancellationToken token)
        {
            var total = guids.Length;
            var current = 0;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start("Deleting Assets", "Moving to Trash...", Progress.Options.Managed);

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(DeleteSelectedAssetsAsync), "Cancel requested by user.");
                cts.Cancel();
                return true;
            });

            var result = await Try.Run(async () =>
            {
                var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();

                if (ShowWarnWindow && !await EditorUtils.DisplayDialogAsync("label.new_delete.title".S(),
                        "label.new_delete.message".Sf(string.Join("\n", paths)), "label.new_delete.ok".S(),
                        "label.new_delete.cancel".S()))
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    return;
                }

                var stopwatch = Stopwatch.StartNew();

                foreach (var path in paths)
                {
                    if (cts.IsCancellationRequested)
                        throw new OperationCanceledException(cts.Token);

                    Try.Run(() =>
                    {
                        var guid = AssetDatabase.AssetPathToGUID(path);
                        var success = AssetDatabase.MoveAssetToTrash(path);
                        if (!success)
                            Utils.LogWarning(nameof(DeleteSelectedAssetsAsync), $"Failed to delete: {path}");
                        else
                            DeleteHistoryManager.Add(new()
                            {
                                path = path,
                                guid = guid,
                                extension = Path.GetExtension(path),
                                unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                            });
                    }).OnFailure(ex => Utils.LogError(nameof(DeleteSelectedAssetsAsync),
                        $"Failed to delete {path}: {ex.Message}\n{ex.StackTrace}"));

                    current++;
                    var progress = (float)current / total;
                    Progress.Report(progressId, progress, $"Deleted: {path}");
                    if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                    await UniTask.Yield(cts.Token);
                    stopwatch.Restart();
                }

                Progress.Finish(progressId);
            });

            result.OnFailure(ex =>
            {
                if (ex is OperationCanceledException)
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    Utils.LogWarning(nameof(DeleteSelectedAssetsAsync), "Delete cancelled.");
                }
                else
                {
                    Progress.Finish(progressId, Progress.Status.Failed);
                    Utils.LogError(nameof(DeleteSelectedAssetsAsync), $"Delete Error: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }
    }
}