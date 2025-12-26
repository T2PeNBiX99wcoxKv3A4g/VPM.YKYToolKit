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
        var shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache");
        if (Directory.Exists(shaderCachePath)) Directory.Delete(shaderCachePath, true);
        Utils.Log(nameof(ClearShaderCache), $"Clearing Shader Cache at {shaderCachePath}");
    }
}