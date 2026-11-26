using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.ykyToolkit.Editor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        /// <summary>
        ///     Shows the expression hint when the field label is hovered. The label keeps the hint off the input box,
        ///     where the suggestion list opens.
        /// </summary>
        private static void AttachExpressionHint(Vector3FieldExtra field)
        {
            var label = field.Q<Label>(className: "unity-base-field__label");
            if (label == null) return;

            const string key = "label.enhanced_transform_inspector.help_hint";
            var helper = LocalizationHelper.For(typeof(EnhancedTransformInspector));

            // Set the tooltip through the helper: UILocalize only looks at a tooltip when the element's own text is
            // a key, and the label shows "Rotation" by the time it runs. Registering keeps the hint in sync with
            // the selected language.
            label.tooltip = helper.S(key);
            var registration = helper.UpdateRegister(key, (text, _) => label.tooltip = text);
            label.RegisterCallback<DetachFromPanelEvent>(_ => registration.Dispose());
        }
    }
}
