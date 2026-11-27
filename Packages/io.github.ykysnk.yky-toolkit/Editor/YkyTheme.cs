using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     The one accent colour every YKY Toolkit window draws with.
    ///     <para>
    ///         The shipped style sheets keep their accent colours behind USS variables with the original colour as the
    ///         fallback (<c>var(--yky-accent, rgb(76, 141, 255))</c>). Unity 2022.3 resolves a variable only inside the
    ///         sheet that declares it, so the variables cannot be defined by a separate sheet. Instead, the source of
    ///         each sheet that uses them is imported again at runtime with the variables replaced by the chosen
    ///         colours, and that copy replaces the shipped sheet in every window.
    ///     </para>
    /// </summary>
    internal static class YkyTheme
    {
        private const string AccentKey = "YKYToolkit/ThemeAccent";

        private static readonly string[] SheetPaths =
        {
            "Packages/io.github.ykysnk.yky-toolkit/Editor/YkyToolkitTheme.uss",
            "Packages/io.github.ykysnk.yky-toolkit/Editor/EnhancedTransformInspector.uss",
            "Packages/io.github.ykysnk.yky-toolkit/Editor/DeleteHistoryWindow.uss",
            "Packages/io.github.ykysnk.yky-toolkit/Editor/ImportWatcherWindow.uss",
            "Packages/io.github.ykysnk.yky-toolkit/Editor/PreferencesPage.uss",
            "Packages/io.github.ykysnk.yky-toolkit/Editor/TransformHelpWindow.uss"
        };

        /// <summary>One <c>var(--yky-…, fallback)</c> call; every accent variable has a plain colour as its fallback.</summary>
        private static readonly Regex AccentVariable =
            new(@"var\((--yky-[a-z0-9-]+),\s*(rgba?\([^)]*\))\)", RegexOptions.Compiled);

        private static readonly List<WeakReference<VisualElement>> Roots = new();

        /// <summary>The cyan-blue accent the toolkit was designed with.</summary>
        internal static readonly Color DefaultAccent = new(76f / 255f, 141f / 255f, 1f, 1f);

        private static Color? _accent;
        private static Dictionary<StyleSheet, StyleSheet>? _sheets;
        private static bool _rebuildScheduled;
        private static bool _importWarningLogged;

        /// <summary>
        ///     Accent colour saved in the user preferences. Alpha is forced to 1, because the colour is used for text
        ///     and borders as well as for fills.
        /// </summary>
        internal static Color Accent
        {
            get
            {
                if (_accent.HasValue) return _accent.Value;

                var stored = EditorPrefs.GetString(AccentKey, "");
                if (string.IsNullOrEmpty(stored) || !ColorUtility.TryParseHtmlString(stored, out var parsed))
                    parsed = DefaultAccent;

                parsed.a = 1f;
                _accent = parsed;
                return parsed;
            }
            set
            {
                value.a = 1f;
                if (_accent.HasValue && IsSameColor(_accent.Value, value)) return;

                _accent = value;
                EditorPrefs.SetString(AccentKey, "#" + ColorUtility.ToHtmlStringRGBA(value));
                ScheduleRebuild();
            }
        }

        /// <summary>Restores the default accent colour.</summary>
        internal static void Reset() => Accent = DefaultAccent;

        /// <summary>
        ///     Swaps the shipped style sheets of <paramref name="root" /> and its children for the accent copies, and
        ///     remembers the root so a colour change reaches windows that are no longer visible as well.
        /// </summary>
        internal static void Apply(VisualElement? root)
        {
            if (root == null) return;

            Remember(root);
            ApplySheets(root);
        }

        private static void Remember(VisualElement root)
        {
            foreach (var reference in Roots)
                if (reference.TryGetTarget(out var known) && known == root)
                    return;

            Roots.Add(new WeakReference<VisualElement>(root));
        }

        private static void ScheduleRebuild()
        {
            if (_rebuildScheduled) return;
            _rebuildScheduled = true;

            // The colour picker reports every step of a drag; rebuilding once per editor tick keeps that cheap.
            EditorApplication.delayCall += () =>
            {
                _rebuildScheduled = false;
                Rebuild();
            };
        }

        private static void Rebuild()
        {
            var previous = _sheets;
            var next = BuildSheets(Accent);

            // The runtime importer is internal to Unity; if a future version refuses it, the previous copy (or the
            // shipped colours) stay in place instead of losing the accent entirely.
            if (next.Count == 0 && previous is { Count: > 0 }) return;

            _sheets = next;

            for (var i = Roots.Count - 1; i >= 0; i--)
            {
                if (!Roots[i].TryGetTarget(out var root))
                {
                    Roots.RemoveAt(i);
                    continue;
                }

                if (previous != null) SwapSheets(root, previous);
                ApplySheets(root);
            }
        }

        private static void SwapSheets(VisualElement root, Dictionary<StyleSheet, StyleSheet> previous)
        {
            foreach (var element in root.Query<VisualElement>().ToList())
            {
                foreach (var pair in previous)
                {
                    if (!element.styleSheets.Contains(pair.Value)) continue;

                    var replacement = _sheets != null && _sheets.TryGetValue(pair.Key, out var next) ? next : pair.Key;
                    ReplaceSheet(element, pair.Value, replacement);
                }
            }
        }

        private static void ApplySheets(VisualElement root)
        {
            var sheets = Sheets;
            if (sheets.Count == 0) return;

            foreach (var element in root.Query<VisualElement>().ToList())
            {
                foreach (var pair in sheets)
                {
                    if (!element.styleSheets.Contains(pair.Key)) continue;
                    ReplaceSheet(element, pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        ///     Swaps one sheet for another while keeping its place in the cascade: the shipped theme has to stay in
        ///     front of the per-window sheets, or its rules would start winning where they overlap.
        /// </summary>
        private static void ReplaceSheet(VisualElement element, StyleSheet from, StyleSheet to)
        {
            var styleSheets = element.styleSheets;

            var index = -1;
            for (var i = 0; i < styleSheets.count; i++)
            {
                if (styleSheets[i] != from) continue;
                index = i;
                break;
            }

            if (index < 0) return;

            var order = new List<StyleSheet>(styleSheets.count);
            for (var i = 0; i < styleSheets.count; i++) order.Add(styleSheets[i]);
            order[index] = to;

            styleSheets.Clear();
            foreach (var sheet in order) styleSheets.Add(sheet);
        }

        private static Dictionary<StyleSheet, StyleSheet> Sheets => _sheets ??= BuildSheets(Accent);

        private static Dictionary<StyleSheet, StyleSheet> BuildSheets(Color accent)
        {
            var result = new Dictionary<StyleSheet, StyleSheet>();
            var useDefaults = IsSameColor(accent, DefaultAccent);

            foreach (var path in SheetPaths)
            {
                var original = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (original == null) continue;

                var text = AccentVariable.Replace(File.ReadAllText(path),
                    match => useDefaults ? match.Groups[2].Value : Css(AccentColor(match.Groups[1].Value, accent)));

                var sheet = BuildStyleSheet(text);
                if (sheet == null) return new();

                sheet.name = original.name + " (Accent)";
                result[original] = sheet;
            }

            return result;
        }

        private static Color AccentColor(string variable, Color accent) => variable switch
        {
            "--yky-accent" => accent,
            "--yky-accent-hover" => Blend(accent, Color.white, 0.12f),
            "--yky-accent-border" => Blend(accent, Color.black, 0.15f),
            "--yky-accent-text" => Blend(accent, Color.white, 0.24f),
            "--yky-accent-text-strong" => Blend(accent, Color.white, 0.54f),
            "--yky-accent-text-soft" => WithAlpha(Blend(accent, Color.white, 0.40f), 0.95f),
            "--yky-accent-line" => WithAlpha(accent, 0.55f),
            "--yky-accent-line-strong" => WithAlpha(accent, 0.72f),
            _ => Tint(variable, accent)
        };

        private static Color Tint(string variable, Color accent)
        {
            const string prefix = "--yky-accent-";
            if (variable.StartsWith(prefix, StringComparison.Ordinal) &&
                int.TryParse(variable[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var alpha))
                return WithAlpha(accent, alpha / 100f);

            return accent;
        }

        private static StyleSheet? BuildStyleSheet(string uss)
        {
            try
            {
                var importerType = FindType("UnityEditor.UIElements.StyleSheets.StyleSheetImporterImpl");
                var import = importerType?.GetMethod("Import", new[] { typeof(StyleSheet), typeof(string) });
                if (import == null) return Unsupported();

                var importer = Activator.CreateInstance(importerType!, true);
                if (importer == null) return Unsupported();

                var sheet = ScriptableObject.CreateInstance<StyleSheet>();
                sheet.hideFlags = HideFlags.HideAndDontSave;
                import.Invoke(importer, new object[] { sheet, uss });
                return sheet;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"YKY Toolkit: the accent colour could not be applied, using the default one. {e.Message}");
                return null;
            }

            StyleSheet? Unsupported()
            {
                if (_importWarningLogged) return null;

                _importWarningLogged = true;
                Debug.LogWarning("YKY Toolkit: this Unity version cannot build the accent style sheets at runtime, " +
                                 "so the toolkit keeps its default accent colour.");
                return null;
            }
        }

        private static Type? FindType(string fullName)
        {
            var type = Type.GetType(fullName + ", UnityEditor.CoreModule");
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }

        private static bool IsSameColor(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 1f / 512f &&
            Mathf.Abs(a.g - b.g) < 1f / 512f &&
            Mathf.Abs(a.b - b.b) < 1f / 512f;

        private static Color Blend(Color from, Color to, float amount) => new(
            Mathf.Lerp(from.r, to.r, amount),
            Mathf.Lerp(from.g, to.g, amount),
            Mathf.Lerp(from.b, to.b, amount),
            from.a);

        private static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

        private static string Css(Color color) => string.Format(CultureInfo.InvariantCulture,
            "rgba({0}, {1}, {2}, {3:0.###})",
            Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255),
            Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255),
            Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255),
            Mathf.Clamp01(color.a));
    }
}
