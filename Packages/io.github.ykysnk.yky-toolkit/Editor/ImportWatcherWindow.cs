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
        private const string EditorKey = "YKYToolkit/ImportWatcher/Settings";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<ImportWatcherFileColor> fileColors = new();
        [SerializeField] private List<ImportSession> sessions = new();

        private static bool SettingsExpanded
        {
            get => EditorPrefs.GetBool(EditorKey);
            set => EditorPrefs.SetBool(EditorKey, value);
        }

        private void OnDestroy()
        {
            fileColors.Rebuild();
            SaveFileColorList();
        }

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            LoadFileColorList();
            fileColors.Rebuild();

            RefreshImportLog();

            var settingsFoldout = tree.Q<Foldout>("settings");
            settingsFoldout.SetValueWithoutNotify(SettingsExpanded);
            settingsFoldout.RegisterValueChangedCallback(evt => SettingsExpanded = evt.newValue);

            var colorField = tree.Q<ColorField>("color");
            colorField.value = ImportWatcher.HighlightColor;
            colorField.RegisterValueChangedCallback(evt => ImportWatcher.HighlightColor = evt.newValue);

            var doubleField = tree.Q<DoubleField>("duration");
            doubleField.value = ImportWatcher.Duration;
            doubleField.RegisterValueChangedCallback(evt => ImportWatcher.Duration = evt.newValue);

            var fileColorsField = tree.Q<ListView>("fileColors");
            var addButton = fileColorsField.Q<Button>("unity-list-view__add-button");
            var removeButton = fileColorsField.Q<Button>("unity-list-view__remove-button");

            addButton.clicked += SaveFileColorList;
            removeButton.clicked += SaveFileColorList;

            var refreshButton = tree.Q<ToolbarButton>("importLogRefresh");
            var clearButton = tree.Q<ToolbarButton>("importLogClear");
            var emptyLabel = tree.Q<Label>("importLogEmpty");
            var listView = tree.Q<ListView>("importLogList");

            refreshButton?.RegisterCallback<ClickEvent>(_ => RefreshImportLog());
            clearButton?.RegisterCallback<ClickEvent>(_ => UniTask.Create(async () =>
            {
                // TODO: Dialog
                if (await EditorUtils.DisplayDialogAsync("Clear Import Log",
                        "Are you sure you want to clear all import log records?", "Clear", "Cancel"))
                {
                    ImportHistoryManager.Clear();
                    RefreshImportLog();
                }
            }));

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

        private void LoadFileColorList()
        {
            fileColors.Clear();
            fileColors.AddRange(ImportWatcher.ColorList);
        }

        private void SaveFileColorList()
        {
            ImportWatcher.ColorList = fileColors;
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

        [MenuItem("Tools/YKYToolkit/Import Watcher Window", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherWindow>();
            window.titleContent = EditorGUIUtils.IconContent(Title, "unityeditor.consolewindow");
        }
    }
}