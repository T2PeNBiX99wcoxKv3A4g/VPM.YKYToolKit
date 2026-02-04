using System;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(DeleteRecord))]
    public class DeleteRecordEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("69729b0a7cef4ba497523fc4402d82d9"));

            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();

            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);

            var guidProperty = property.FindPropertyRelative("guid");
            var extensionProperty = property.FindPropertyRelative("extension");
            var unixSecondsProperty = property.FindPropertyRelative("unixSeconds");

            var guidLabel = tree.Q<Label>("guid");
            var extLabel = tree.Q<Label>("ext");
            var timeLabel = tree.Q<Label>("time");

            UpdateLabels();
            tree.TrackPropertyValue(property, _ => UpdateLabels());

            return tree;

            void UpdateLabels()
            {
                guidLabel.text =
                    $"{(string.IsNullOrEmpty(guidProperty.stringValue) ? "label.delete_record.unknown".S() : guidProperty.stringValue)}";
                extLabel.text =
                    $"{(string.IsNullOrEmpty(extensionProperty.stringValue) ? "label.delete_record.none".S() : extensionProperty.stringValue)}";

                var time = DateTimeOffset.FromUnixTimeSeconds(unixSecondsProperty.longValue).LocalDateTime
                    .ToString("yyyy-MM-dd HH:mm:ss");
                timeLabel.text = time;
            }
        }
    }
}