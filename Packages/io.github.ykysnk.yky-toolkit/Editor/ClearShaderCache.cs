using System.IO;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEngine;
using Progress = UnityEditor.Progress;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class ClearShaderCache
    {
        [MenuItem("Tools/YKYToolkit/Clear Shader Cache")]
        private static void Clear() => ClearAsync().Forget();

        private static async UniTask ClearAsync()
        {
            if (!await EditorUtils.DisplayDialogAsync("Clear Shader Cache",
                    "Are you sure want to clear the shader cache?\nUnity may rebuild all shader cache when unity editor is start.\nThis action can't be undone!",
                    "Yes", "No")) return;
            var shaderCachePath =
                Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? "", "Library/ShaderCache");
            Utils.Log(nameof(ClearShaderCache), $"Shader Cache Path: {shaderCachePath}");
            var progressId = Progress.Start("Clearing shader Cache...", "Preparing...",
                Progress.Options.Sticky | Progress.Options.Indefinite);
            Progress.Report(progressId, 0, "Clearing...");
            await UniTask.RunOnThreadPool(() =>
            {
                if (Directory.Exists(shaderCachePath))
                    Directory.Delete(shaderCachePath, true);
            });
            Progress.Finish(progressId);
            await EditorUtils.DisplayDialogAsync("Shader Cache Cleared", "Shader Cache has been cleared.");
            AssetDatabase.Refresh();
        }
    }
}