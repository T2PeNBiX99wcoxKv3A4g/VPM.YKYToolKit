using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Read-only row for the transform database. The values are changed from the Transform inspector, so this
    ///     view only lists which objects have extra data.
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
            return root;
        }
    }
}
