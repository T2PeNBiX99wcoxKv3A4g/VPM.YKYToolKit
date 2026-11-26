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
                list.style.height = size > 0 ? size * 28 + 4 : 0;
                empty.style.display = size > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            }

            tree.Bind(serializedObject);
            RefreshSummary();
            return tree;
        }
    }
}
