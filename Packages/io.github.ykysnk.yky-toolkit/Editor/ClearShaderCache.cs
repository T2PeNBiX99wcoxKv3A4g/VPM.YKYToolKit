using System.IO;
using io.github.ykysnk.utils;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor;

internal static class ClearShaderCache
{
    [MenuItem($"Tools/{Util.Name}/Clear Shader Cache")]
    private static void Clear()
    {
        if (!EditorUtility.DisplayDialog("Clear Shader Cache", "Are you sure you want to clear the Shader Cache?", "Yes",
                "No")) return;
        var shaderCachePath =
            Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? "", "Library/ShaderCache");
        Utils.Log(nameof(ClearShaderCache), $"Shader Cache Path: {shaderCachePath}");
        if (Directory.Exists(shaderCachePath)) Directory.Delete(shaderCachePath, true);
        EditorUtility.DisplayDialog("Shader Cache Cleared", "Shader Cache has been cleared.", "OK");
        AssetDatabase.Refresh();
    }
}