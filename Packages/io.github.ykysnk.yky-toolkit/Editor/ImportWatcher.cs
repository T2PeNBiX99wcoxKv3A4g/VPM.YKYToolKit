using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    // TODO: import log, Editor Settings
    internal class ImportWatcher : AssetPostprocessor
    {
        private const double DefaultDuration = 120;
        private const string ImportHighlights = "YKYToolkit/ImportWatcher/ImportHighlights";
        private const string ImportHighlightColor = "YKYToolkit/ImportWatcher/Color";
        private const string ImportHighlightDuration = "YKYToolkit/ImportWatcher/Duration";
        internal const string ImportHighlightFileColor = "YKYToolkit/ImportWatcher/FileColor";
        private static readonly HashSet<HighlightInfo> Highlights = new();
        internal static readonly Color DefaultHighlightColor = new(1f, 0f, 0f, 0.10f);

        static ImportWatcher()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;

            if (!Load(out var infos)) return;
            Highlights.UnionWith(infos);
        }

        internal static Color HighlightColor
        {
            get => ColorUtility.TryParseHtmlString(EditorPrefs.GetString(ImportHighlightColor), out var color)
                ? color
                : DefaultHighlightColor;
            set => EditorPrefs.SetString(ImportHighlightColor, $"#{ColorUtility.ToHtmlStringRGBA(value)}");
        }

        internal static double Duration
        {
            get => EditorPrefs.GetFloat(ImportHighlightDuration, (float)DefaultDuration);
            set => EditorPrefs.SetFloat(ImportHighlightDuration, (float)value);
        }

        private static List<ImportWatcherFileColor> FileColors
        {
            get
            {
                var json = EditorPrefs.GetString(ImportHighlightFileColor);

                if (string.IsNullOrEmpty(json) ||
                    !JsonUtils.TryFromJson<ListWrapper<ImportWatcherFileColor>>(json, out var colors, out _))
                    return new(ImportWatcherFileColor.DefaultColors);

                return colors!.items;
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets.Length < 1 && movedAssets.Length < 1) return;

            CleanupExpired();
            CleanupMissingAssets();

            foreach (var path in importedAssets)
                AddOrUpdate(path);

            foreach (var path in movedAssets)
                AddOrUpdate(path);

            Save();
        }

        private static void AddOrUpdate(string path)
        {
            var add = new HighlightInfo(path, EditorApplication.timeSinceStartup + Duration);

            if (Highlights.Contains(add))
                Highlights.Remove(add);

            Highlights.Add(add);
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

        private static Color GetColorForAsset(string path)
        {
            var fileColor = FileColors.FirstOrDefault(x => Path.GetExtension(path) == x.fileExtension);
            return fileColor?.color ?? ImportWatcherFileColor.DefaultColor;
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
            => AssetDatabase.GetMainAssetTypeAtPath(path) != null;

        private static void Save()
        {
            if (!JsonUtils.TryToJson(Wrapper.Create(Highlights), out var json, out _)) return;
            PlayerPrefs.SetString(ImportHighlights, json);
        }

        private static bool Load(out List<HighlightInfo> infos)
        {
            infos = new();
            if (!JsonUtils.TryFromJson<ListWrapper<HighlightInfo>>(PlayerPrefs.GetString(ImportHighlights, ""),
                    out var get, out _)) return false;
            infos.AddRange(get!);
            return true;
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
    }
}