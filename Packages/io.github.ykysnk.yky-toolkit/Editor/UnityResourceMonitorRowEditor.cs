using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(UnityResourceMonitorRow))]
    public class UnityResourceMonitorRowEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("40e54acba952f1c46964c0e4ef8d8cb4"));
            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();
            var tree = uxml.CloneTree();
            tree.Bind(property.serializedObject);

            var maxValue = property.FindPropertyRelative("maxValue").intValue;
            var bar = tree.Q<ProgressBar>("bar");
            var progressFill = bar.Q(null, "unity-progress-bar__progress");
            bar.highValue = maxValue;

            var valueLabel = tree.Q<Label>("value");
            tree.schedule.Execute(UpdateUI).Every(100);
            UpdateUI();

            return tree;

            void UpdateUI()
            {
                if (bar.value > (float)maxValue / 2)
                {
                    valueLabel.style.color = new Color(1f, 0.5f, 0);
                    progressFill.style.backgroundColor = new Color(1f, 0.5f, 0);
                }
                else if (bar.value > maxValue)
                {
                    valueLabel.style.color = new Color(0.5f, 0, 0);
                    progressFill.style.backgroundColor = new Color(0.5f, 0, 0);
                }
                else
                {
                    valueLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                    progressFill.style.backgroundColor = new Color(0, 0.5f, 0);
                }
            }
        }
    }
}