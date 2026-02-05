using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(ImportRecord))]
    public class ImportRecordEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("8aa74d169e294628a39565139e8f9629"));

            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();

            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);

            var guidProperty = property.FindPropertyRelative("guid");
            var nameProperty = property.FindPropertyRelative("name");
            var pathProperty = property.FindPropertyRelative("path");
            var isFolderProperty = property.FindPropertyRelative("isFolder");

            var iconImage = tree.Q<Image>("icon");
            var nameLabel = tree.Q<Label>("name");

            UpdateUI();
            tree.TrackPropertyValue(property, _ => UpdateUI());

            tree.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount != 2) return;
                var asset = AssetDatabase.LoadAssetAtPath<Object>(
                    AssetDatabase.GUIDToAssetPath(guidProperty.stringValue));
                if (asset == null) return;
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            });

            return tree;

            void UpdateUI()
            {
                var path = pathProperty.stringValue;
                var fileName = nameProperty.stringValue;

                nameLabel.text = fileName;
                nameLabel.tooltip = path;

                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    iconImage.image = AssetDatabase.GetCachedIcon(path);
                    nameLabel.style.color = StyleKeyword.Null;
                }
                else
                {
                    nameLabel.style.color = Color.red;
                    iconImage.image = InternalEditorUtility.GetIconForFile(path);
                }
            }
        }
    }
}