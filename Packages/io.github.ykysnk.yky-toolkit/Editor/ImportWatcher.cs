using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class ImportWatcher : AssetPostprocessor
    {
        private const int DefaultDuration = 120;
        private const string ImportHighlights = "YKYToolkit/ImportWatcher/ImportHighlights";
        private const string ImportHighlightColor = "YKYToolkit/ImportWatcher/Color";
        private const string ImportHighlightDuration = "YKYToolkit/ImportWatcher/Duration";
        private const string ImportHighlightFileColor = "YKYToolkit/ImportWatcher/FileColor";
        private const int HighlightRadius = 5;
        private const float GridHeight = 24f;
        private const double FadeIn = 0.30;
        private const double FadeOut = 1.20;
        private static readonly HashSet<HighlightInfo> Highlights = new();
        private static readonly Dictionary<string, HighlightInfo> PathLookup = new();
        private static readonly HashSet<string> HighlightFolders = new();
        private static readonly Color DefaultHighlightColor = new(1f, 0f, 0f, 0.10f);

        private static GUIStyle? _fillStyle;
        private static GUIStyle? _outlineStyle;
        private static string _fileColorsSource = "";
        private static Dictionary<string, Color> _fileColorLookup = new();
        private static string _highlightColorSource = "";
        private static Color _highlightColor = DefaultHighlightColor;
        private static double _nextRepaint;

        static ImportWatcher()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
            EditorApplication.update += OnEditorUpdate;

            if (!Load(out var infos)) return;
            Highlights.UnionWith(infos);
            RebuildPathCache();
        }

        internal static Color HighlightColor
        {
            get => ColorUtility.TryParseHtmlString(EditorPrefs.GetString(ImportHighlightColor), out var color)
                ? color
                : DefaultHighlightColor;
            set
            {
                EditorPrefs.SetString(ImportHighlightColor, $"#{ColorUtility.ToHtmlStringRGBA(value)}");
                EditorApplication.RepaintProjectWindow();
            }
        }

        internal static int Duration
        {
            get => EditorPrefs.GetInt(ImportHighlightDuration, DefaultDuration);
            set => EditorPrefs.SetInt(ImportHighlightDuration, value);
        }

        internal static void ResetSettings()
        {
            HighlightColor = DefaultHighlightColor;
            Duration = DefaultDuration;
        }

        internal static List<ImportWatcherFileColor> ColorList
        {
            get
            {
                var json = EditorPrefs.GetString(ImportHighlightFileColor);
                if (!JsonUtils.TryFromJson<ListWrapper<ImportWatcherFileColor>>(json, out var colors, out _))
                    return new(ImportWatcherFileColor.DefaultColors);
                return new(colors!);
            }
            set
            {
                if (!JsonUtils.TryToJson(Wrapper.Create(value), out var json, out _)) return;
                EditorPrefs.SetString(ImportHighlightFileColor, json);
                EditorApplication.RepaintProjectWindow();
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets.Length > 0 || movedAssets.Length > 0)
            {
                CleanupExpired();
                CleanupMissingAssets();

                var session = new ImportSession();

                foreach (var path in importedAssets)
                {
                    AddOrUpdate(path);
                    session.records.Add(new(AssetDatabase.AssetPathToGUID(path)));
                }

                foreach (var path in movedAssets)
                    AddOrUpdate(path);

                if (session.records.Count > 0)
                    ImportHistoryManager.AddSession(session);

                RebuildPathCache();
                Save();

                // The highlight fades in, so start the repaints right away instead of waiting for the window.
                EditorApplication.RepaintProjectWindow();
            }

            if (deletedAssets.Length <= 0) return;

            var records = new List<DeleteRecord>(deletedAssets.Length);
            foreach (var path in deletedAssets)
                records.Add(new()
                {
                    path = path,
                    guid = AssetDatabase.AssetPathToGUID(path),
                    extension = Path.GetExtension(path),
                    unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

            DeleteHistoryManager.AddRange(records);
        }

        // The project window item callback runs for every visible item on every repaint, so the lookups use sets
        // instead of walking the highlight list (and building "path/" strings) each time.
        private static void RebuildPathCache()
        {
            PathLookup.Clear();
            HighlightFolders.Clear();

            foreach (var highlight in Highlights)
            {
                PathLookup[highlight.path] = highlight;

                var folder = Path.GetDirectoryName(highlight.path);
                while (!string.IsNullOrEmpty(folder) && HighlightFolders.Add(folder))
                    folder = Path.GetDirectoryName(folder);
            }
        }

        private static void AddOrUpdate(string path)
        {
            var add = new HighlightInfo(path);

            if (Highlights.Contains(add))
                Highlights.Remove(add);

            Highlights.Add(add);
        }

        private static void OnProjectItemGUI(string guid, Rect rect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var isFile = PathLookup.TryGetValue(path, out var info);

            if (!isFile && !HighlightFolders.Contains(path)) return;
            if (rect.width < 6f || rect.height < 6f) return;
            if (Event.current == null || Event.current.type != EventType.Repaint) return;

            var now = Now;
            if (isFile && info!.expireTime <= now) return;

            var wash = CurrentHighlightColor;
            var strength = isFile ? Fade(info!, now) : 0.45f;
            wash.a *= strength;

            var body = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);
            DrawRounded(body, wash, FillStyle);

            if (!isFile) return;

            var tint = GetColorForAsset(path);
            tint.a = Mathf.Clamp01(tint.a * 1.2f) * strength;

            // The icon view frames the whole cell. The list view gets an underline instead, which marks the row
            // without drawing a frame around every imported file.
            if (rect.height > GridHeight)
                DrawRounded(body, tint, OutlineStyle);
            else
                EditorGUI.DrawRect(new Rect(rect.x + 4f, rect.yMax - 3f, rect.width - 8f, 2f), tint);
        }

        private static void OnEditorUpdate()
        {
            if (Highlights.Count == 0) return;

            var now = Now;
            if (Highlights.RemoveWhere(h => h.expireTime <= now) > 0)
            {
                RebuildPathCache();
                Save();
                EditorApplication.RepaintProjectWindow();
                return;
            }

            var animating = false;

            foreach (var highlight in Highlights)
            {
                var life = Math.Max(0.2, highlight.expireTime - highlight.createdTime);
                var fadingIn = highlight.createdTime > 0 &&
                               now - highlight.createdTime < Math.Min(FadeIn, life * 0.25);
                var fadingOut = highlight.expireTime - now < Math.Min(FadeOut, Math.Max(0.2, life * 0.33));

                if (!fadingIn && !fadingOut) continue;

                animating = true;
                break;
            }

            if (!animating || EditorApplication.timeSinceStartup < _nextRepaint) return;

            _nextRepaint = EditorApplication.timeSinceStartup + 1.0 / 30.0;
            EditorApplication.RepaintProjectWindow();
        }

        private static double Now => (DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds;

        /// <summary>The colour from the settings, cached until the stored value changes.</summary>
        private static Color CurrentHighlightColor
        {
            get
            {
                var source = EditorPrefs.GetString(ImportHighlightColor, "");
                if (source == _highlightColorSource) return _highlightColor;

                _highlightColorSource = source;
                _highlightColor = ColorUtility.TryParseHtmlString(source, out var parsed)
                    ? parsed
                    : DefaultHighlightColor;
                return _highlightColor;
            }
        }

        /// <summary>Fades a highlight in when it is new and out again before it expires.</summary>
        private static float Fade(HighlightInfo info, double now)
        {
            var life = Math.Max(0.2, info.expireTime - info.createdTime);
            var fadeIn = info.createdTime > 0
                ? Mathf.Clamp01((float)((now - info.createdTime) / Math.Min(FadeIn, life * 0.25)))
                : 1f;
            var fadeOut = Mathf.Clamp01((float)((info.expireTime - now) /
                                                Math.Min(FadeOut, Math.Max(0.2, life * 0.33))));
            return Mathf.Min(fadeIn, fadeOut);
        }

        private static void DrawRounded(Rect rect, Color color, GUIStyle style)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;

            var previous = GUI.color;
            GUI.color = color;
            style.Draw(rect, false, false, false, false);
            GUI.color = previous;
        }

        private static GUIStyle FillStyle => _fillStyle ??= CreateRoundedStyle(CreateRoundedTexture(false));

        private static GUIStyle OutlineStyle => _outlineStyle ??= CreateRoundedStyle(CreateRoundedTexture(true));

        private static GUIStyle CreateRoundedStyle(Texture2D texture)
        {
            var border = HighlightRadius + 1;
            var style = new GUIStyle
            {
                border = new RectOffset(border, border, border, border)
            };
            style.normal.background = texture;
            return style;
        }

        /// <summary>A white rounded rectangle (filled or as a 1.5px outline) for the nine-sliced highlight.</summary>
        private static Texture2D CreateRoundedTexture(bool outline)
        {
            const int inset = 1;
            const int samples = 4;
            var size = (HighlightRadius + inset) * 2 + 4;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = outline ? "YKY Import Highlight Outline" : "YKY Import Highlight",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color[size * size];
            var min = inset;
            var max = size - inset;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var coverage = 0f;

                for (var sy = 0; sy < samples; sy++)
                for (var sx = 0; sx < samples; sx++)
                {
                    var px = x + (sx + 0.5f) / samples;
                    var py = y + (sy + 0.5f) / samples;

                    var inside = InRoundedRect(px, py, min, min, max, max, HighlightRadius);
                    if (outline)
                        inside &= !InRoundedRect(px, py, min + 1.5f, min + 1.5f, max - 1.5f, max - 1.5f,
                            HighlightRadius - 1.5f);

                    if (inside) coverage += 1f / (samples * samples);
                }

                pixels[y * size + x] = new Color(1f, 1f, 1f, coverage);
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static bool InRoundedRect(float x, float y, float left, float top, float right, float bottom,
            float radius)
        {
            var closestX = Mathf.Clamp(x, left + radius, right - radius);
            var closestY = Mathf.Clamp(y, top + radius, bottom - radius);
            var dx = x - closestX;
            var dy = y - closestY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static Color GetColorForAsset(string path)
        {
            var source = EditorPrefs.GetString(ImportHighlightFileColor);
            if (source != _fileColorsSource) RefreshFileColors(source);

            return _fileColorLookup.TryGetValue(Path.GetExtension(path), out var color)
                ? color
                : ImportWatcherFileColor.DefaultColor;
        }

        private static void RefreshFileColors(string source)
        {
            _fileColorsSource = source;
            _fileColorLookup = new();

            if (string.IsNullOrEmpty(source) ||
                !JsonUtils.TryFromJson<ListWrapper<ImportWatcherFileColor>>(source, out var colors, out _))
                return;

            foreach (var entry in colors!.items)
                if (!string.IsNullOrEmpty(entry.fileExtension))
                    _fileColorLookup[entry.fileExtension] = entry.color;
        }

        private static void CleanupExpired()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
            EditorPrefs.SetString(ImportHighlights, json);
        }

        private static bool Load(out List<HighlightInfo> infos)
        {
            infos = new();
            if (!JsonUtils.TryFromJson<ListWrapper<HighlightInfo>>(EditorPrefs.GetString(ImportHighlights, ""),
                    out var get, out _)) return false;
            infos.AddRange(get!);
            return true;
        }

        [Serializable]
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        private class HighlightInfo : IEquatable<HighlightInfo>
        {
            public string path;
            public long expireTime;
            public double createdTime;

            public HighlightInfo(string path)
            {
                this.path = path;
                createdTime = Now;
                expireTime = (long)(createdTime + Duration);
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