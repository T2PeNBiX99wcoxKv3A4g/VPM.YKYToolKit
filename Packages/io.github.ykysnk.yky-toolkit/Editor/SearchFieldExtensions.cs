using System;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class SearchFieldExtensions
    {
        private const string PlaceholderKey = "label.search";
        private const string ClearGlyph = "×";

        /// <summary>
        ///     The toolbar search field has no placeholder and the shared button style clears the magnifier, so this
        ///     restores the look and puts the hint on the tooltip.
        ///     <para>
        ///         The field is read back through <paramref name="changed" /> instead of a value-changed callback that
        ///         the caller registers: a docked window restored in a background tab can come back holding nothing
        ///         but the clear glyph, which is not a search term and would filter every row away.
        ///     </para>
        /// </summary>
        internal static void SetupSearchField(this ToolbarSearchField field, Action<string> changed)
        {
            field.AddToClassList("yky-search");

            // The localizer must not touch the field: on a restored window it pushes a translation into the
            // field's value, the filter then runs with it and hides every row.
            field.AddToClassList("localize-skip");

            field.tooltip = PlaceholderKey.S();

            if (field.Q<Button>("unity-cancel") is { } cancel) cancel.text = ClearGlyph;

            field.RegisterValueChangedCallback(evt =>
            {
                var text = string.IsNullOrEmpty(evt.newValue) || evt.newValue == ClearGlyph
                    ? string.Empty
                    : evt.newValue;

                if (text != evt.newValue) field.SetValueWithoutNotify(text);
                changed(text);
            });
        }
    }
}
