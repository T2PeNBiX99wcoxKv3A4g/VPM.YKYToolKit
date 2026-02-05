using System;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(ImportSession))]
    public class ImportSessionEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("1456c6e22b467c241b975a27b5777f89"));
            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();
            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);

            var unixSecondsProperty = property.FindPropertyRelative("unixSeconds");

            var time = DateTimeOffset.FromUnixTimeSeconds(unixSecondsProperty.longValue).LocalDateTime
                .ToString("yyyy-MM-dd HH:mm:ss");

            var recordsListView = tree.Q<ListView>("records");
            var foldout = recordsListView.Q<Foldout>();
            foldout.text = $"{time}";
            foldout.value = true;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            var sizeTextField = recordsListView.Q<TextField>("unity-list-view__size-field");
            sizeTextField.isReadOnly = true;

            return tree;
        }
    }
}