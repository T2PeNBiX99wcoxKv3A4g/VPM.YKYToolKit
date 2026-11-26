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
        ///     A toolbar search field has no placeholder and the shared button style clears the magnifier, so this adds
        ///     both and turns the clear button into a visible "x".
        /// </summary>
        internal static void SetupSearchField(this ToolbarSearchField field)
        {
            field.AddToClassList("yky-search");

            var placeholder = new Label(PlaceholderKey.S()) { pickingMode = PickingMode.Ignore };
            placeholder.AddToClassList("yky-search__placeholder");
            field.Add(placeholder);

            var registration = PlaceholderKey.Register((text, _) => placeholder.text = text);
            field.RegisterCallback<DetachFromPanelEvent>(_ => registration.Dispose());

            void Update() =>
                placeholder.style.display = string.IsNullOrEmpty(field.value) ? DisplayStyle.Flex : DisplayStyle.None;

            field.RegisterValueChangedCallback(_ => Update());
            Update();

            if (field.Q<Button>("unity-cancel") is { } cancel)
            {
                cancel.text = "×";
                cancel.AddToClassList("yky-search__cancel");
            }
        }
    }
}
