using io.github.ykysnk.utils.Editor;
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
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("c26a6854be5c7d242b661c85b140896a"));
            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);
            return tree;
        }
    }
}