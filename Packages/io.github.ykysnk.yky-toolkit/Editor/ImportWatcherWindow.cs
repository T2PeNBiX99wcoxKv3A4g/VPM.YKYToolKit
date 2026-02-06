using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class ImportWatcherWindow : EditorWindow
    {
        private const string Title = "Import Watcher";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<ImportSession> sessions = new();

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            RefreshImportLog();

            var refreshButton = tree.Q<ToolbarButton>("refresh");
            var clearButton = tree.Q<ToolbarButton>("clear");
            var settingsButton = tree.Q<ToolbarButton>("settings");
            var emptyLabel = tree.Q<Label>("importLogEmpty");
            var listView = tree.Q<ListView>("importLogList");

            refreshButton.clicked += RefreshImportLog;
            clearButton.clicked += () => UniTask.Create(async () =>
            {
                if (await EditorUtils.DisplayDialogAsync("label.import_watcher.clear_import_log_title".S(),
                        "label.import_watcher.clear_import_log_message".S(),
                        "label.clear".S(),
                        "label.cancel".S()))
                {
                    ImportHistoryManager.Clear();
                    RefreshImportLog();
                }
            });
            settingsButton.clicked += ImportWatcherSettingsWindow.ShowWindow;

            UpdateVisibility();
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(_ => UpdateVisibility());
            return;

            void UpdateVisibility()
            {
                if (emptyLabel != null)
                    emptyLabel.style.display = sessions.Count > 0 ? DisplayStyle.None : DisplayStyle.Flex;
                if (listView != null)
                    listView.style.display = sessions.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RefreshImportLog()
        {
            sessions.Clear();
            sessions.AddRange(ImportHistoryManager.All());
            sessions.Reverse();
            sessions.Rebuild();

            var emptyLabel = rootVisualElement.Q<Label>("importLogEmpty");
            var listView = rootVisualElement.Q<ListView>("importLogList");
            if (emptyLabel != null)
                emptyLabel.style.display = sessions.Count > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            if (listView != null)
                listView.style.display = sessions.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherWindow>();
            window.titleContent = EditorGUIUtils.IconContent(Title, "undohistory");
        }
    }
}