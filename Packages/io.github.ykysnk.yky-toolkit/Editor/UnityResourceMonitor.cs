using System.Collections.Generic;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class UnityResourceMonitor : EditorWindow
    {
        private const string Title = "Unity Resource Monitor";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<UnityResourceMonitorRow> rows = new();

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml?.CloneTree();
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            rootVisualElement.AddManipulator(
                new ContextualMenuManipulator(evt => evt.menu.AppendAction("Reload", _ => Reload())));

            rootVisualElement.schedule.Execute(UpdateUI).Every(100);
            UpdateUI();
            return;

            void UpdateUI()
            {
                foreach (var row in rows)
                    row.Update();
            }
        }

        private void Reload()
        {
            rows.Clear();
            rows.Add(new UserHandlesMonitorRow());
            rows.Add(new GdiHandlesMonitorRow());
            rows.Add(new MonoUsedMonitorRow());
            rows.Add(new GCAllocatedMonitorRow());
            rows.Add(new TotalAllocatedMonitorRow());
        }

        [MenuItem("Tools/YKYToolkit/Unity Resource Monitor")]
        private static void ShowWindow()
        {
            var window = GetWindow<UnityResourceMonitor>();

            window.titleContent = EditorGUIUtils.IconContent(Title, "Profiler.Memory");

            if (window.rows.Count < 1)
                window.Reload();
        }
    }
}