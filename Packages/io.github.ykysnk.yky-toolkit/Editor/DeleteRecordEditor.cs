using System;
using System.IO;
using io.github.ykysnk.utils;
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

            var pathLabel = tree.Q<Label>("path");
            var guidLabel = tree.Q<Label>("guid");
            var extLabel = tree.Q<Label>("ext");
            var timeLabel = tree.Q<Label>("time");

            UpdateLabels();
            tree.TrackPropertyValue(property, _ => UpdateLabels());

            tree.Q<Button>("copyPath").clicked += () => EditorGUIUtility.systemCopyBuffer = pathLabel.text;
            tree.Q<Button>("copyGuid").clicked += () => EditorGUIUtility.systemCopyBuffer = guidProperty.stringValue;

            tree.Q<Button>("ping").clicked += () =>
            {
                var path = pathLabel.text;
                var obj = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
                else
                    Utils.LogWarning(nameof(DeleteRecordEditor), $"Asset not found in project: {path}");
            };

            tree.Q<Button>("reveal").clicked += () =>
            {
                var path = pathLabel.text;
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    EditorUtility.RevealInFinder(dir);
                else
                    Utils.LogWarning(nameof(DeleteRecordEditor), $"Folder not found: {dir}");
            };

            return tree;

            void UpdateLabels()
            {
                guidLabel.text =
                    $"{(string.IsNullOrEmpty(guidProperty.stringValue) ? "(unknown)" : guidProperty.stringValue)}";
                extLabel.text =
                    $"{(string.IsNullOrEmpty(extensionProperty.stringValue) ? "(none)" : extensionProperty.stringValue)}";

                var time = DateTimeOffset.FromUnixTimeSeconds(unixSecondsProperty.longValue).LocalDateTime
                    .ToString("yyyy-MM-dd HH:mm:ss");
                timeLabel.text = time;
            }
        }
    }
}