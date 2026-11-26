using io.github.ykysnk.Localization.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
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

            AddPrecision(root, property.FindPropertyRelative("positionDecimalPrecision"), "P",
                "label.enhanced_transform_inspector.position_decimal_precision");
            AddPrecision(root, property.FindPropertyRelative("rotationDecimalPrecision"), "R",
                "label.enhanced_transform_inspector.rotation_decimal_precision");
            AddPrecision(root, property.FindPropertyRelative("scaleDecimalPrecision"), "S",
                "label.enhanced_transform_inspector.scale_decimal_precision");

            var lockToggle = new Toggle
            {
                bindingPath = property.FindPropertyRelative("lockTransform").propertyPath,
                tooltip = "label.enhanced_transform_inspector.lock_transform".S()
            };
            lockToggle.AddToClassList("transform-db-entry__lock");
            root.Add(lockToggle);

            root.Bind(property.serializedObject);
            return root;
        }

        private static void AddPrecision(VisualElement root, SerializedProperty property, string label,
            string tooltipKey)
        {
            var field = new IntegerField(label)
            {
                isDelayed = true,
                bindingPath = property.propertyPath,
                tooltip = tooltipKey.S()
            };
            field.AddToClassList("transform-db-entry__precision");
            root.Add(field);
        }
    }
}
