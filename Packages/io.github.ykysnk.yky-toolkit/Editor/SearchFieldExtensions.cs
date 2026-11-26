using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class SearchFieldExtensions
    {
        private const string PlaceholderKey = "label.search";

        /// <summary>
        ///     The toolbar search field has no placeholder and the shared button style clears the magnifier, so this
        ///     restores the look and puts the hint on the tooltip.
        ///     <para>
        ///         The placeholder is deliberately not a Label inside the field: in this Unity version the field can
        ///         read an added text element as its own value, which fired the search callback and filtered every
        ///         list entry away after a reload.
        ///     </para>
        /// </summary>
        internal static void SetupSearchField(this ToolbarSearchField field)
        {
            field.AddToClassList("yky-search");
            field.tooltip = PlaceholderKey.S();

            if (field.Q<Button>("unity-cancel") is { } cancel)
                cancel.text = "×";
        }
    }
}
