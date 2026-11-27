using System;
using System.Collections.Generic;
using io.github.ykysnk.Localization.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>Standalone page with the transform expression cheat sheet and a search box.</summary>
    internal class TransformHelpWindow : EditorWindow
    {
        private const string Title = "Transform Expression Help";
        private const string UxmlPath = "Packages/io.github.ykysnk.yky-toolkit/Editor/TransformHelpWindow.uxml";
        private const string CopiedKey = "label.enhanced_transform_inspector.help_copied";

        private readonly List<GroupSection> _sections = new();
        private Label? _empty;

        private void CreateGUI()
        {
            _sections.Clear();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (uxml == null)
            {
                rootVisualElement.Add(new Label(UxmlPath));
                return;
            }

            var tree = uxml.CloneTree();
            tree.style.flexGrow = 1;
            YkyTheme.Apply(tree);
            rootVisualElement.Add(tree);

            var groups = tree.Q<VisualElement>("groups");
            _empty = tree.Q<Label>("empty");

            foreach (var group in new[]
                     {
                         TransformExpressionHelp.Group.Basic,
                         TransformExpressionHelp.Group.Order,
                         TransformExpressionHelp.Group.Random
                     })
            {
                var section = new VisualElement();
                section.AddToClassList("yky-help__group");

                var title = new Label(TransformExpressionHelp.GroupKey(group));
                title.AddToClassList("yky-help__group-title");
                section.Add(title);

                var model = new GroupSection { Element = section };

                foreach (var entry in TransformExpressionHelp.Entries)
                {
                    if (entry.Group != group) continue;

                    var row = CreateRow(entry);
                    section.Add(row.Element);
                    model.Rows.Add(row);
                }

                groups.Add(section);
                _sections.Add(model);
            }

            tree.UILocalize(false);

            var search = tree.Q<ToolbarSearchField>("search");
            SetupSearch(search);
            search.RegisterValueChangedCallback(evt => Filter(evt.newValue));

            Filter(search.value);

            // The localization assets load after this window is built; a pass that still reaches the field must
            // not leave every row hidden, so the filter is applied once more with whatever the field holds then.
            tree.schedule.Execute(() => Filter(search.value)).ExecuteLater(50);
        }

        /// <summary>Sets up the search field: styling, the clear glyph and its own tooltip instead of a placeholder.</summary>
        private static void SetupSearch(ToolbarSearchField search)
        {
            search.AddToClassList("yky-search");

            // The localizer must not touch the field: on a restored window it pushes a translation into the
            // field's value, the filter then runs with it and hides every row.
            search.AddToClassList("localize-skip");

            const string key = "label.search";
            var helper = LocalizationHelper.For(typeof(TransformHelpWindow));
            search.tooltip = helper.S(key);
            var registration = helper.UpdateRegister(key, (text, _) => search.tooltip = text);
            search.RegisterCallback<DetachFromPanelEvent>(_ => registration.Dispose());

            if (search.Q<Button>("unity-cancel") is { } cancel)
                cancel.text = "×";
        }

        private void Filter(string query)
        {
            var text = query?.Trim() ?? "";
            var any = false;

            foreach (var section in _sections)
            {
                var visible = 0;

                foreach (var row in section.Rows)
                {
                    var match = text.Length == 0 || Matches(row.Entry, text);
                    row.Element.style.display = match ? DisplayStyle.Flex : DisplayStyle.None;
                    if (match) visible++;
                }

                section.Element.style.display = visible > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                any |= visible > 0;
            }

            if (_empty != null)
                _empty.style.display = any ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private static bool Matches(TransformExpressionHelp.Entry entry, string query)
        {
            // Syntax and the function alias always count. The description only joins in from two characters so a
            // single letter such as "r" does not match half of the page through words like "start" or "order".
            if (entry.Syntax.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (entry.Sample.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (entry.Alt.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return query.Length >= 2 &&
                   entry.DescriptionKey.S().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Row CreateRow(TransformExpressionHelp.Entry entry)
        {
            var element = new VisualElement();
            element.AddToClassList("yky-help__row");
            element.tooltip = string.IsNullOrEmpty(entry.Signature) ? entry.Sample : entry.Signature;

            var syntax = new Label(entry.Syntax);
            syntax.AddToClassList("yky-help__syntax");
            element.Add(syntax);

            var description = new Label(entry.DescriptionKey);
            description.AddToClassList("yky-help__description");
            element.Add(description);

            if (!string.IsNullOrEmpty(entry.Alt))
            {
                var alt = new Label(entry.Alt);
                alt.AddToClassList("yky-help__alt");
                element.Add(alt);
            }

            var copied = false;

            element.RegisterCallback<ClickEvent>(_ =>
            {
                if (copied) return;
                copied = true;

                EditorGUIUtility.systemCopyBuffer = entry.Sample;

                var original = description.text;
                description.text = CopiedKey.S();
                description.AddToClassList("yky-help__copied");

                element.schedule.Execute(() =>
                {
                    description.text = original;
                    description.RemoveFromClassList("yky-help__copied");
                    copied = false;
                }).StartingIn(900);
            });

            return new Row { Entry = entry, Element = element };
        }

        [MenuItem("Tools/YKYToolkit/Transform Expression Help", false, Util.Three)]
        internal static void Open()
        {
            var window = GetWindow<TransformHelpWindow>();

            var icon = EditorGUIUtility.FindTexture("_Help");
            window.titleContent = icon == null ? new GUIContent(Title) : new GUIContent(Title, icon);
            window.minSize = new Vector2(380, 420);
            window.Show();
        }

        private sealed class Row
        {
            internal TransformExpressionHelp.Entry Entry;
            internal VisualElement Element;
        }

        private sealed class GroupSection
        {
            internal VisualElement Element;
            internal readonly List<Row> Rows = new();
        }
    }
}
