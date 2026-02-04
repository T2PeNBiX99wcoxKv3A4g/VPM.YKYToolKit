using System.IO;
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

            var pathProperty = property.FindPropertyRelative("path");
            var nameProperty = property.FindPropertyRelative("name");
            var isFolderProperty = property.FindPropertyRelative("isFolder");

            var iconImage = tree.Q<Image>("icon");
            var nameLabel = tree.Q<Label>("name");
            var timeLabel = tree.Q<Label>("time");
            timeLabel.style.display = DisplayStyle.None;

            void UpdateUI()
            {
                var path = pathProperty.stringValue;
                var fileName = nameProperty.stringValue;
                if (string.IsNullOrEmpty(fileName)) fileName = Path.GetFileName(path);

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
                    if (isFolderProperty.boolValue)
                        iconImage.image = EditorGUIUtility.IconContent("Folder Icon").image;
                    else
                        iconImage.image = InternalEditorUtility.GetIconForFile(path);
                }
            }

            UpdateUI();
            tree.TrackPropertyValue(pathProperty, _ => UpdateUI());
            tree.TrackPropertyValue(nameProperty, _ => UpdateUI());

            tree.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(pathProperty.stringValue);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }
            });

            return tree;
        }
    }
}