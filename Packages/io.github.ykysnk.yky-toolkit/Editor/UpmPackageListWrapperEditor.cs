using System.Collections.Generic;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(UpmPackageListWrapper))]
    public class UpmPackageListWrapperEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    AssetDatabase.GUIDToAssetPath("7271de2a351dc6441afe753991cb0c25"));

            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();

            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);

            var importButton = tree.Q<Button>("import");
            importButton.clicked += () =>
            {
                var indexStr = property.propertyPath.MiddlePath('[', ']');
                if (!int.TryParse(indexStr, out var index)) return;
                var list = (List<UpmPackageListWrapper>)fieldInfo.GetValue(property.serializedObject.targetObject);
                UpmInstallerWindow.ImportPackages(list[index]);
            };

            var foldout = tree.Q<Foldout>();
            foldout.schedule.Execute(() => foldout.value = false);

            return tree;
        }
    }
}