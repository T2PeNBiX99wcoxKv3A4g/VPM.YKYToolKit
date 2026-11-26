using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using io.github.ykysnk.Localization.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        private const int MaxSuggestions = 7;
        private const float SuggestionWidth = 252f;
        private const string StyleSheetPath =
            "Packages/io.github.ykysnk.yky-toolkit/Editor/EnhancedTransformInspector.uss";

        private readonly List<TransformExpressionHelp.Entry> _suggestionEntries = new();
        private VisualElement? _suggestionPopup;
        private VisualElement? _suggestionList;
        private FloatField? _suggestionField;
        private int _suggestionIndex = -1;
        private bool _suggestionVisible;
        private string _suggestionTyped = "";

        /// <summary>
        ///     Offers the expression list under the axis field while the user types a prefix that is not a complete
        ///     expression yet. Enter or Tab inserts the highlighted example, the first number is selected so it can be
        ///     replaced right away.
        /// </summary>
        private void AttachExpressionSuggestions(FloatField field)
        {
            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (!IsSuggesting(field)) return;

                switch (evt.keyCode)
                {
                    case KeyCode.DownArrow:
                        MoveSuggestion(1);
                        Consume(evt);
                        break;
                    case KeyCode.UpArrow:
                        MoveSuggestion(-1);
                        Consume(evt);
                        break;
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                    case KeyCode.Tab:
                        AcceptSuggestion();
                        Consume(evt);
                        break;
                    case KeyCode.Escape:
                        HideSuggestions();
                        Consume(evt);
                        break;
                }
            }, TrickleDown.TrickleDown);

            field.RegisterCallback<KeyUpEvent>(evt =>
            {
                // Navigation keys do not change the text; rebuilding the list would reset the highlighted row.
                switch (evt.keyCode)
                {
                    case KeyCode.DownArrow:
                    case KeyCode.UpArrow:
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                    case KeyCode.Tab:
                    case KeyCode.Escape:
                    case KeyCode.LeftShift:
                    case KeyCode.RightShift:
                    case KeyCode.LeftControl:
                    case KeyCode.RightControl:
                    case KeyCode.LeftAlt:
                    case KeyCode.RightAlt:
                        return;
                }

                UpdateSuggestions(field);
            }, TrickleDown.TrickleDown);
            field.RegisterCallback<FocusOutEvent>(_ =>
                _suggestionPopup?.schedule.Execute(HideSuggestions).ExecuteLater(1));

            static void Consume(EventBase evt)
            {
                evt.StopImmediatePropagation();
                evt.PreventDefault();
            }
        }

        private bool IsSuggesting(FloatField field) =>
            _suggestionVisible &&
            _suggestionPopup != null &&
            _suggestionField == field &&
            _suggestionEntries.Count > 0;

        private void UpdateSuggestions(FloatField field)
        {
            var typed = field.text?.Trim() ?? "";

            // A complete expression or a plain number is what Enter already applies; a popup there would only get in
            // the way, and it would also cover the value the user is looking at.
            if (typed.Length == 0 ||
                TransformInputParser.Parse(typed).Success ||
                float.TryParse(typed, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                HideSuggestions();
                return;
            }

            // Same prefix and the popup is already open: keep the list and its highlighted row.
            if (_suggestionVisible && _suggestionField == field && typed == _suggestionTyped) return;

            var matches = MatchSuggestions(typed);
            if (matches.Count == 0)
            {
                HideSuggestions();
                return;
            }

            ShowSuggestions(field, typed, matches);
        }

        private static List<TransformExpressionHelp.Entry> MatchSuggestions(string typed)
        {
            var result = new List<TransformExpressionHelp.Entry>();

            foreach (var entry in TransformExpressionHelp.Entries)
                if (Matches(entry, typed))
                    result.Add(entry);

            // Typing "R" should offer R(...) before RR(...); an exact name match wins over a longer prefix.
            return result
                .OrderBy(entry => MatchScore(entry, typed))
                .Take(MaxSuggestions)
                .ToList();
        }

        private static int MatchScore(TransformExpressionHelp.Entry entry, string typed)
        {
            var sample = entry.Sample;
            var open = sample.IndexOf('(');
            var name = open > 0 ? sample[..open] : sample[..1];

            if (string.Equals(name, typed, System.StringComparison.OrdinalIgnoreCase)) return 0;
            if (name.StartsWith(typed, System.StringComparison.OrdinalIgnoreCase)) return 1;
            return 2;
        }

        private static bool Matches(TransformExpressionHelp.Entry entry, string typed)
        {
            var sample = entry.Sample;
            var open = sample.IndexOf('(');
            var name = open > 0 ? sample[..open] : sample[..1];

            return sample.StartsWith(typed, System.StringComparison.OrdinalIgnoreCase) ||
                   name.StartsWith(typed, System.StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrEmpty(entry.Alt) &&
                    entry.Alt.StartsWith(typed, System.StringComparison.OrdinalIgnoreCase)) ||
                   entry.Signature.StartsWith(typed, System.StringComparison.OrdinalIgnoreCase);
        }

        private void ShowSuggestions(FloatField field, string typed, List<TransformExpressionHelp.Entry> matches)
        {
            var windowRoot = GetWindowRoot(field);
            if (windowRoot == null) return;

            if (_suggestionPopup == null)
            {
                _suggestionPopup = new VisualElement
                {
                    name = "expressionSuggestions",
                    pickingMode = PickingMode.Position
                };
                _suggestionPopup.AddToClassList("transform-suggest");

                // The popup lives on the window root rather than inside the cloned inspector tree, so it does not
                // inherit the UXML's style sheet; attach the same sheet explicitly.
                var sheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
                if (sheet != null)
                    _suggestionPopup.styleSheets.Add(sheet);

                _suggestionList = new VisualElement();
                _suggestionList.AddToClassList("transform-suggest__list");
                _suggestionPopup.Add(_suggestionList);

                var footer = new Label("label.enhanced_transform_inspector.help_suggest");
                footer.AddToClassList("transform-suggest__footer");
                _suggestionPopup.Add(footer);
                _suggestionPopup.UILocalize(false);
            }

            if (_suggestionPopup.parent != windowRoot)
            {
                _suggestionPopup.RemoveFromHierarchy();
                windowRoot.Add(_suggestionPopup);
            }

            _suggestionField = field;
            _suggestionTyped = typed;
            _suggestionEntries.Clear();
            _suggestionEntries.AddRange(matches);

            _suggestionList!.Clear();
            foreach (var entry in matches)
                _suggestionList.Add(CreateSuggestionRow(entry));

            HighlightSuggestion(0);

            var input = field.Q<VisualElement>("unity-text-input") ?? field;
            var bound = input.worldBound;
            var origin = windowRoot.worldBound.position;
            var windowWidth = windowRoot.worldBound.width;
            var windowHeight = windowRoot.worldBound.height;

            var estimatedHeight = matches.Count * 18f + 30f;
            var top = bound.yMax - origin.y + 2f;
            if (top + estimatedHeight > windowHeight)
                top = Mathf.Max(1f, bound.yMin - origin.y - estimatedHeight - 2f);

            _suggestionPopup.style.width = SuggestionWidth;
            _suggestionPopup.style.left = Mathf.Clamp(bound.xMin - origin.x, 1f,
                Mathf.Max(1f, windowWidth - SuggestionWidth - 4f));
            _suggestionPopup.style.top = top;
            _suggestionPopup.style.display = DisplayStyle.Flex;
            _suggestionPopup.BringToFront();
            _suggestionVisible = true;
        }

        /// <summary>
        ///     The window content root. Children added to the panel's own visual tree are not laid out, so the popup
        ///     has to live one level below it.
        /// </summary>
        private static VisualElement? GetWindowRoot(VisualElement field)
        {
            var panel = field.panel;
            if (panel == null) return null;

            VisualElement current = field;
            while (current.parent != null && current.parent != panel.visualTree)
                current = current.parent;

            return current.parent == panel.visualTree ? current : panel.visualTree;
        }

        private VisualElement CreateSuggestionRow(TransformExpressionHelp.Entry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("transform-suggest__row");

            var syntax = new Label(entry.Sample);
            syntax.AddToClassList("transform-suggest__syntax");
            row.Add(syntax);

            var description = new Label(entry.DescriptionKey.S());
            description.AddToClassList("transform-suggest__description");
            row.Add(description);

            row.RegisterCallback<PointerEnterEvent>(_ =>
                HighlightSuggestion(_suggestionList!.IndexOf(row)));

            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopPropagation();
                HighlightSuggestion(_suggestionList!.IndexOf(row));
                _suggestionField?.Focus();
                AcceptSuggestion();
            }, TrickleDown.TrickleDown);

            return row;
        }

        private void HighlightSuggestion(int index)
        {
            if (_suggestionList == null) return;

            var rows = _suggestionList.Children().ToList();
            if (rows.Count == 0) return;

            _suggestionIndex = Mathf.Clamp(index, 0, rows.Count - 1);

            for (var i = 0; i < rows.Count; i++)
                rows[i].EnableInClassList("transform-suggest__row--selected", i == _suggestionIndex);
        }

        private void MoveSuggestion(int delta)
        {
            if (_suggestionList == null) return;

            var count = _suggestionList.childCount;
            if (count == 0) return;

            var next = (_suggestionIndex + delta + count) % count;
            HighlightSuggestion(next);
        }

        private void AcceptSuggestion()
        {
            var field = _suggestionField;
            if (field == null || _suggestionIndex < 0 || _suggestionIndex >= _suggestionEntries.Count) return;

            var entry = _suggestionEntries[_suggestionIndex];
            HideSuggestions();

            // Defer one frame: the Enter or Tab that got us here still runs the field's own commit, which would
            // replace the inserted text. The field already has focus (the user was typing), so no focus event either.
            field.schedule.Execute(() => InsertSuggestion(field, entry)).ExecuteLater(0);
        }

        private static void InsertSuggestion(FloatField field, TransformExpressionHelp.Entry entry)
        {
            // FloatField.text has no public setter in 2022.3, and the element named 'unity-text-input' is the
            // FloatInput control; its own text element is what the user sees and edits. Selecting a range only
            // works while that element is the focused one.
            var text = field.Q<VisualElement>("unity-text-input")?.Q<TextElement>();
            if (text == null) return;

            text.Focus();
            text.text = entry.Sample;

            var start = FirstNumber(entry.Sample);
            if (start < 0) return;

            var end = start;
            while (end < entry.Sample.Length && (char.IsDigit(entry.Sample[end]) || entry.Sample[end] == '.'))
                end++;

            text.selection.SelectRange(start, end);
        }

        private static int FirstNumber(string sample)
        {
            for (var i = 0; i < sample.Length; i++)
                if (char.IsDigit(sample[i]))
                    return i;

            return -1;
        }

        private void HideSuggestions()
        {
            _suggestionVisible = false;

            if (_suggestionPopup == null) return;

            _suggestionPopup.style.display = DisplayStyle.None;
            _suggestionField = null;
            _suggestionTyped = "";
            _suggestionEntries.Clear();
            _suggestionIndex = -1;
        }

        private void RemoveSuggestionPopup()
        {
            _suggestionPopup?.RemoveFromHierarchy();
            _suggestionPopup = null;
            _suggestionList = null;
            _suggestionField = null;
            _suggestionTyped = "";
            _suggestionEntries.Clear();
            _suggestionVisible = false;
        }
    }
}
