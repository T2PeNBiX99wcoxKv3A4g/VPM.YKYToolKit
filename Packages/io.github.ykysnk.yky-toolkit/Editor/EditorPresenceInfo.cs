using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>Project-level facts shown in the Discord presence; none of it describes a specific asset.</summary>
    internal static class EditorPresenceInfo
    {
        private const string VrchatBasePath = "Packages/com.vrchat.base";

        internal static string ProjectName { get; } = GetProjectName();

        /// <summary>The VRChat SDK and its project type when present, otherwise the active build target.</summary>
        internal static string Context { get; } = GetContext();

        internal static string LargeText { get; } =
            $"Unity {Application.unityVersion} · {GetRenderPipelineName()}";

        private static string GetProjectName()
        {
            var dataPath = Application.dataPath;
            if (string.IsNullOrEmpty(dataPath)) return Application.productName;

            var directory = Path.GetDirectoryName(dataPath);
            return string.IsNullOrEmpty(directory) ? Application.productName : Path.GetFileName(directory);
        }

        private static string GetContext()
        {
            try
            {
                var version = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(VrchatBasePath)?.version;
                if (!string.IsNullOrEmpty(version))
                {
                    var root = Path.GetDirectoryName(Application.dataPath);
                    var hasWorlds = root != null &&
                                    Directory.Exists(Path.Combine(root, "Packages", "com.vrchat.worlds"));
                    var hasAvatars = root != null &&
                                     Directory.Exists(Path.Combine(root, "Packages", "com.vrchat.avatars"));

                    var kind = hasWorlds && hasAvatars
                        ? "VRChat"
                        : hasWorlds
                            ? "VRChat World"
                            : hasAvatars
                                ? "VRChat Avatar"
                                : "VRChat SDK";

                    return $"{kind} {version}";
                }
            }
            catch (Exception)
            {
                // Package manager is not ready during some reloads; the build target is a fine fallback.
            }

            return EditorUserBuildSettings.activeBuildTarget.ToString();
        }

        private static string GetRenderPipelineName()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline == null) return "Built-in";

            var name = pipeline.GetType().Name;
            if (name.Contains("Universal")) return "URP";
            if (name.Contains("HDRenderPipeline") || name.Contains("HighDefinition")) return "HDRP";

            return name.Replace("RenderPipelineAsset", "");
        }
    }
}
