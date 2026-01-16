using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using Object = UnityEngine.Object;
using Progress = UnityEditor.Progress;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class NewDelete
    {
        private const string ToolsMenuPath = "Tools/YKYToolkit/Delete Selected Warn Window";
        private const string AssetsMenuPath = "Assets/YKYToolkit/Delete Selected Warn Window";

        private static bool ShowWarnWindow
        {
            set => EditorPrefs.SetBool("YKYToolkit/DeleteSelectedWarnWindow", value);
            get => EditorPrefs.GetBool("YKYToolkit/DeleteSelectedWarnWindow", true);
        }

        [MenuItem(ToolsMenuPath, false, Util.DeleteSelectedPriority)]
        [MenuItem(AssetsMenuPath)]
        private static void DeleteSelectedShowMenu() => ShowWarnWindow = !ShowWarnWindow;

        [MenuItem(ToolsMenuPath, true, Util.DeleteSelectedPriority)]
        [MenuItem(AssetsMenuPath, true)]
        private static bool DeleteSelectedGameObjectsShowMenuValidate()
        {
            Menu.SetChecked(ToolsMenuPath, ShowWarnWindow);
            Menu.SetChecked(AssetsMenuPath, ShowWarnWindow);
            return true;
        }

        [MenuItem("GameObject/YKYToolkit/Delete Selected", false, Util.DeleteSelectedPriority)]
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

            try
            {
                foreach (var selectedObject in selectedObjects)
                {
                    if (selectedObject == null) continue;
                    var name = selectedObject.name;

                    if (cts.IsCancellationRequested)
                        throw new OperationCanceledException(cts.Token);

                    try
                    {
                        Undo.DestroyObjectImmediate(selectedObject);
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(nameof(DeleteSelectedObjectsAsync),
                            $"Failed to delete {name}: {ex.Message}\n{ex.StackTrace}");
                    }

                    current++;
                    var progress = (float)current / total;
                    Progress.Report(progressId, progress, $"Deleted: {name}");
                    await UniTask.NextFrame(cts.Token);
                }

                Progress.Finish(progressId);
            }
            catch (OperationCanceledException)
            {
                Progress.Finish(progressId, Progress.Status.Canceled);
                Utils.LogWarning(nameof(DeleteSelectedObjectsAsync), "Delete cancelled.");
            }
            catch (Exception ex)
            {
                Progress.Finish(progressId, Progress.Status.Failed);
                Utils.LogError(nameof(DeleteSelectedObjectsAsync), $"Delete Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Progress.UnregisterCancelCallback(progressId);
            }
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

            try
            {
                var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();

                if (ShowWarnWindow && !await EditorUtils.DisplayDialogAsync("Delete select asset",
                        $"{string.Join("\n", paths)}\n\nYou can undo the delete assets action. (Not 100%)", "Delete",
                        "Cancel"))
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    return;
                }

                foreach (var path in paths)
                {
                    if (cts.IsCancellationRequested)
                        throw new OperationCanceledException(cts.Token);

                    try
                    {
                        byte[]? backup = null;

                        if (File.Exists(path))
                            backup = await File.ReadAllBytesAsync(path, cts.Token);

                        if (backup != null)
                            Undo.undoRedoPerformed += () => UniTask.Create(async () =>
                            {
                                if (!File.Exists(path))
                                {
                                    await File.WriteAllBytesAsync(path, backup, cts.Token);
                                    AssetDatabase.ImportAsset(path);
                                    await UniTask.NextFrame(cts.Token);
                                }
                                else
                                {
                                    if (!AssetDatabase.MoveAssetToTrash(path))
                                        Utils.LogWarning(nameof(DeleteSelectedAssetsAsync),
                                            $"Failed to delete: {path}");
                                }
                            });

                        if (!AssetDatabase.MoveAssetToTrash(path))
                            Utils.LogWarning(nameof(DeleteSelectedAssetsAsync), $"Failed to delete: {path}");
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(nameof(DeleteSelectedAssetsAsync),
                            $"Failed to delete {path}: {ex.Message}\n{ex.StackTrace}");
                    }

                    current++;
                    var progress = (float)current / total;
                    Progress.Report(progressId, progress, $"Deleted: {path}");
                    await UniTask.NextFrame(cts.Token);
                }

                Progress.Finish(progressId);
            }
            catch (OperationCanceledException)
            {
                Progress.Finish(progressId, Progress.Status.Canceled);
                Utils.LogWarning(nameof(DeleteSelectedAssetsAsync), "Delete cancelled.");
            }
            catch (Exception ex)
            {
                Progress.Finish(progressId, Progress.Status.Failed);
                Utils.LogError(nameof(DeleteSelectedAssetsAsync), $"Delete Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Progress.UnregisterCancelCallback(progressId);
            }
        }
    }
}