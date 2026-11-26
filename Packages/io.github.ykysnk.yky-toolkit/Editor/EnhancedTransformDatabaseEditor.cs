using System.Linq;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomEditor(typeof(EnhancedTransformDatabase))]
    public class EnhancedTransformDatabaseEditor : BasicEditor
    {
        [SerializeField] private VisualTreeAsset? uxml;

        protected override VisualElement? CreateErrorHandleInspectorGUI()
        {
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);

            var list = tree.Q<ListView>("entries");
            var count = tree.Q<Label>("count");
            var empty = tree.Q<Label>("empty");
            var remove = tree.Q<Button>("remove");

            list.makeItem = () =>
            {
                var field = new PropertyField { name = "entry" };
                field.AddToClassList("transform-db__item");
                return field;
            };
            list.bindItem = (element, index) =>
            {
                var entry = serializedObject.FindProperty("entries").GetArrayElementAtIndex(index);
                var field = (PropertyField)element;
                field.Unbind();
                field.bindingPath = entry.propertyPath;
                field.Bind(serializedObject);
            };

            void RefreshSummary()
            {
                serializedObject.Update();
                var size = serializedObject.FindProperty("entries")?.arraySize ?? 0;

                count.text = size.ToString();
                list.style.display = size > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                list.style.height = Mathf.Clamp(size * 28 + 8, 60, 420);
                empty.style.display = size > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            }

            void RefreshRemoveButton() => remove.SetEnabled(list.selectedIndices.Any());

            list.selectionChanged += _ => RefreshRemoveButton();

            remove.clicked += () =>
            {
                // Remove from the last index first, or every earlier index shifts and deletes the wrong entry.
                var selected = list.selectedIndices.OrderByDescending(index => index).ToList();
                if (selected.Count == 0) return;

                serializedObject.Update();
                var entries = serializedObject.FindProperty("entries");

                foreach (var index in selected)
                    entries.DeleteArrayElementAtIndex(index);

                serializedObject.ApplyModifiedProperties();
                list.Rebuild();
                RefreshSummary();
                RefreshRemoveButton();
            };

            tree.Bind(serializedObject);
            RefreshSummary();
            RefreshRemoveButton();
            return tree;
        }
    }
}
