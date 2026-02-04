using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(ImportWatcherFileColor))]
    public class ImportWatcherFileColorEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("6bdd0e10034ff1b4281322ac210d8519"));
            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();
            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);
            return tree;
        }
    }
}