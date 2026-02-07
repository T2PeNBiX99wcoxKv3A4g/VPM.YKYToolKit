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
            if (!await EditorUtils.DisplayDialogAsync("label.clear_shader_cache.title".S(),
                    "label.clear_shader_cache.message".S(), "label.clear_shader_cache.ok".S(),
                    "label.clear_shader_cache.cancel".S(), 3)) return;
            var shaderCachePath = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? "",
                "Library/ShaderCache");
            Utils.Log(nameof(ClearShaderCache), $"Shader Cache Path: {shaderCachePath}");
            var progressId = Progress.Start("Clearing shader Cache...", "Preparing...",
                Progress.Options.Managed | Progress.Options.Indefinite);
            Progress.Report(progressId, 0, "Clearing...");
            await UniTask.RunOnThreadPool(() =>
            {
                if (Directory.Exists(shaderCachePath))
                    Directory.Delete(shaderCachePath, true);
            });
            Progress.Finish(progressId);
            await EditorUtils.DisplayDialogAsync("label.clear_shader_cache.title2".S(),
                "label.clear_shader_cache.message2".S());
            AssetDatabase.Refresh();
        }
    }
}