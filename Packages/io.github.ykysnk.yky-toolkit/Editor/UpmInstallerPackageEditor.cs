using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(UpmInstallerPackage))]
    public class UpmInstallerPackageEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    AssetDatabase.GUIDToAssetPath("8938dc6d412e46d42aeb92c2573da8b2"));

            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();

            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);

            var packageNameField = tree.Q<TextField>("packageName");
            var packageNamePlaceholderLabel = packageNameField.Q<Label>(null, "placeholder");
            packageNameField.RegisterValueChangedCallback(_ =>
                packageNamePlaceholderLabel.style.display = string.IsNullOrEmpty(packageNameField.value)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);

            var versionField = tree.Q<TextField>("version");
            var versionPlaceholderLabel = versionField.Q<Label>(null, "placeholder");
            versionField.RegisterValueChangedCallback(_ =>
                versionPlaceholderLabel.style.display =
                    string.IsNullOrEmpty(versionField.value) ? DisplayStyle.Flex : DisplayStyle.None);

            return tree;
        }
    }
}