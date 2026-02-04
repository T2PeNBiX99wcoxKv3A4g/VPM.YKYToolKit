using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class DeleteHistoryWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<DeleteRecord> records = new();

        private ToolbarButton? _clearButton;
        private Label? _emptyLabel;
        private ListView? _listView;
        private ToolbarButton? _refreshButton;

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);

            Refresh();

            _refreshButton = tree.Q<ToolbarButton>("refreshButton");
            _refreshButton.clicked += Refresh;

            _clearButton = tree.Q<ToolbarButton>("clearButton");
            _clearButton.clicked += OnClearClicked;

            _emptyLabel = tree.Q<Label>("emptyLabel");
            _listView = tree.Q<ListView>("listView");
            _listView.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (!_listView.selectedIndices.Any() || _listView.selectedIndex < 0)
                    return;

                var selected = records[_listView.selectedIndex];

                evt.menu.AppendAction("label.delete_record.copy_path".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = selected.path);
                evt.menu.AppendAction("label.delete_record.copy_guid".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = selected.guid);
            }));

            UpdateVisibility();
        }

        [MenuItem("Tools/YKYToolkit/Delete History", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<DeleteHistoryWindow>();
            window.titleContent = EditorGUIUtils.IconContent("Delete History", "undohistory");
            window.minSize = new(520, 280);
        }

        private void OnClearClicked()
        {
            if (!EditorUtility.DisplayDialog(
                    "Clear Delete History",
                    "Are you sure you want to clear all delete history records?",
                    "Clear", "Cancel"))
                return;
            DeleteHistoryManager.Clear();
            Refresh();
        }

        private void Refresh()
        {
            records.Clear();
            records.AddRange(DeleteHistoryManager.All());
            records.Reverse();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (_emptyLabel != null)
                _emptyLabel.style.display = records.Count > 0 ? DisplayStyle.None : DisplayStyle.Flex;

            if (_listView != null)
                _listView.style.display = records.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}