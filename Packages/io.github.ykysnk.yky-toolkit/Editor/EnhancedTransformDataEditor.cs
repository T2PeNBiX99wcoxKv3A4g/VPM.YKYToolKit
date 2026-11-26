using io.github.ykysnk.Localization.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Read-only row for the transform database. The values are changed from the Transform inspector, so this
    ///     view only lists what is stored.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnhancedTransformData))]
    public class EnhancedTransformDataEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddToClassList("transform-db-entry");

            var id = property.FindPropertyRelative("id").stringValue;
            var path = new Label(EnhancedTransformData.DisplayName(id));
            path.AddToClassList("transform-db-entry__path");
            path.tooltip = path.text;

            if (EnhancedTransformData.Resolve(id) == null)
                path.AddToClassList("transform-db-entry__path--missing");

            root.Add(path);

            AddValue(root, "P", property.FindPropertyRelative("positionDecimalPrecision").intValue,
                "label.enhanced_transform_inspector.position_decimal_precision");
            AddValue(root, "R", property.FindPropertyRelative("rotationDecimalPrecision").intValue,
                "label.enhanced_transform_inspector.rotation_decimal_precision");
            AddValue(root, "S", property.FindPropertyRelative("scaleDecimalPrecision").intValue,
                "label.enhanced_transform_inspector.scale_decimal_precision");

            return root;
        }

        private static void AddValue(VisualElement root, string label, int value, string tooltipKey)
        {
            var text = new Label($"{label} {value}");
            text.AddToClassList("transform-db-entry__value");
            text.tooltip = tooltipKey.S();
            root.Add(text);
        }
    }
}
