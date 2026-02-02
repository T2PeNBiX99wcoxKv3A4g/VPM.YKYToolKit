using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    // TODO: Menu for settings
    internal class ImportWatcher : AssetPostprocessor
    {
        private const double Duration = 120;

        private static readonly HashSet<HighlightInfo> Highlights = new();
        private static readonly Color HighlightColor = new(1f, 0f, 0f, 0.12f);

        static ImportWatcher()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;

            if (!TryGetImportHighlights(out var infos, out _)) return;
            Highlights.UnionWith(infos);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets.Length < 1 && movedAssets.Length < 1) return;

            CleanupExpired();
            CleanupMissingAssets();

            foreach (var path in importedAssets)
                Highlights.Add(new(path, EditorApplication.timeSinceStartup + Duration));

            foreach (var path in movedAssets)
                Highlights.Add(new(path, EditorApplication.timeSinceStartup + Duration));

            Save();
        }

        // TODO: Move to utils
        private static bool TryGetImportHighlights(out HighlightInfo[] infos, out Exception? exception)
        {
            infos = Array.Empty<HighlightInfo>();
            exception = null;

            try
            {
                var get = JsonUtility.FromJson<HighlightInfos>(PlayerPrefs.GetString("YKYToolkit/ImportHighlights", ""));
                infos = get.infos;
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        private static void OnProjectItemGUI(string guid, Rect rect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (!Highlights.Any(h => h.path == path || h.path.StartsWith(path + "/"))) return;
            EditorGUI.DrawRect(rect, HighlightColor);

            var tint = GetColorForAsset(path);
            var bar = new Rect(rect.x, rect.y, 2, rect.height);
            EditorGUI.DrawRect(bar, tint);
        }

        // TODO: Make a menu
        private static Color GetColorForAsset(string path)
        {
            return path switch
            {
                _ when path.EndsWith(".cs") => new(1f, 0.85f, 0.2f, 0.45f),
                _ when path.EndsWith(".prefab") => new(0.4f, 0.6f, 1f, 0.45f),
                _ when path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".tga") => new(0.4f, 1f,
                    0.4f, 0.45f),
                _ when path.EndsWith(".mat") => new(0.8f, 0.4f, 1f, 0.45f),
                _ when path.EndsWith(".anim") => new(1f, 0.5f, 0.3f, 0.45f),
                _ => new(1, 1, 1, 0.45f)
            };
        }

        private static void CleanupExpired()
        {
            var now = EditorApplication.timeSinceStartup;
            Highlights.RemoveWhere(h => now > h.expireTime);
        }

        private static void CleanupMissingAssets()
        {
            Highlights.RemoveWhere(h => !AssetExists(h.path));
        }

        private static bool AssetExists(string path)
            => AssetDatabase.LoadAssetAtPath<Object>(path) != null;

        private static void Save()
        {
            PlayerPrefs.SetString("YKYToolkit/ImportHighlights",
                JsonUtility.ToJson(new HighlightInfos(Highlights.ToArray())));
        }

        [Serializable]
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        private class HighlightInfo : IEquatable<HighlightInfo>
        {
            public string path;
            public double expireTime;

            public HighlightInfo(string path, double expireTime)
            {
                this.path = path;
                this.expireTime = expireTime;
            }

            public bool Equals(HighlightInfo? other)
            {
                if (other is null)
                    return false;
                return path == other.path;
            }

            public override int GetHashCode() => path.GetHashCode();

            public override bool Equals(object? obj) => Equals((HighlightInfo?)obj);
        }

        [Serializable]
        private class HighlightInfos
        {
            public HighlightInfo[] infos;
            public HighlightInfos(HighlightInfo[] infos) => this.infos = infos;
        }
    }
}