using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(ImportSession))]
    public class ImportSessionEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.style.marginBottom = 8;
            root.style.marginTop = 4;
            root.style.borderBottomWidth = 1;
            root.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            var unixSecondsProperty = property.FindPropertyRelative("unixSeconds");
            var recordsProperty = property.FindPropertyRelative("records");

            var time = DateTimeOffset.FromUnixTimeSeconds(unixSecondsProperty.longValue).LocalDateTime
                .ToString("yyyy-MM-dd HH:mm:ss");

            var foldout = new Foldout();
            foldout.text = $"{time} ({recordsProperty.arraySize} items)";
            foldout.value = true;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            var listView = new ListView();
            listView.bindingPath = "records";
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.selectionType = SelectionType.Multiple;
            listView.style.marginLeft = 16;
            listView.showBoundCollectionSize = false;

            foldout.Add(listView);
            root.Add(foldout);

            return root;
        }
    }
}