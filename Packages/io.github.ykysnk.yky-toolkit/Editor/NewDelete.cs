using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.Localization.Editor;
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

        internal static void ResetSettings() => ShowWarnWindow = true;

        [MenuItem(ToolsMenuPath, false, Util.Two)]
        [MenuItem(AssetsMenuPath)]
        private static void DeleteSelectedShowMenu() => ShowWarnWindow = !ShowWarnWindow;

        [MenuItem(ToolsMenuPath, true, Util.Two)]
        [MenuItem(AssetsMenuPath, true)]
        private static bool DeleteSelectedGameObjectsShowMenuValidate()
        {
            Menu.SetChecked(ToolsMenuPath, ShowWarnWindow);
            Menu.SetChecked(AssetsMenuPath, ShowWarnWindow);
            return true;
        }

        [MenuItem("GameObject/YKYToolkit/Delete Selected", false, Util.Two)]
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
                var paths = NormalizePaths(guids.Select(AssetDatabase.GUIDToAssetPath));

                if (ShowWarnWindow && !await EditorUtils.DisplayDialogAsync("label.new_delete.title".S(),
                        new DeleteDialogContent(paths),
                        "label.new_delete.ok".S(),
                        "label.new_delete.cancel".S()))
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    return;
                }

                var existing = paths.Where(AssetExists).ToList();

                if (existing.Count > 0)
                {
                    var failed = new List<string>();

                    // One native call is about 2.5x faster than moving one asset at a time, and the progress bar
                    // cannot repaint during it anyway. The batch is deliberately not chunked: every chunk pays the
                    // asset database refresh again.
                    Try.Run(() => AssetDatabase.MoveAssetsToTrash(existing.ToArray(), failed)).OnFailure(ex =>
                        Utils.LogError(nameof(DeleteSelectedAssetsAsync),
                            $"Failed to delete {existing.Count} assets: {ex.Message}\n{ex.StackTrace}"));

                    foreach (var path in failed)
                        Utils.LogWarning(nameof(DeleteSelectedAssetsAsync), $"Failed to delete: {path}");
                }

                // The delete history is filled by ImportWatcher, which sees every deleted asset, including the ones
                // removed by Unity itself. Recording here as well duplicated every entry.
                Progress.Report(progressId, 1f, $"Deleted {existing.Count} assets");
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

        /// <summary>
        ///     Drops duplicates and any child of a selected folder, so the children are not moved twice and recorded
        ///     twice. The list is ordered parents first.
        /// </summary>
        private static List<string> NormalizePaths(IEnumerable<string> paths)
        {
            var unique = paths.Where(path => !string.IsNullOrEmpty(path)).Distinct().ToList();
            unique.Sort((a, b) => a.Length.CompareTo(b.Length));

            var result = new List<string>(unique.Count);
            foreach (var path in unique)
            {
                var underSelectedFolder = result.Any(parent =>
                    path.Length > parent.Length + 1 &&
                    path.StartsWith(parent + "/", StringComparison.OrdinalIgnoreCase) &&
                    AssetDatabase.IsValidFolder(parent));

                if (!underSelectedFolder) result.Add(path);
            }

            return result;
        }

        private static bool AssetExists(string path) =>
            AssetDatabase.IsValidFolder(path) || AssetDatabase.GetMainAssetTypeAtPath(path) != null;
    }
}
