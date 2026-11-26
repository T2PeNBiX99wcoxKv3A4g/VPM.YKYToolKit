using System.Collections.Generic;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class ImportWatcherSettingsWindow : EditorWindow
    {
        private const string Title = "Import Watcher Settings";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<ImportWatcherFileColor> fileColors = new();

        private SerializedObject? _serializedObject;

        private void OnDestroy()
        {
            fileColors.Rebuild();
            SaveFileColorList();
            _serializedObject?.Dispose();
        }

        private void CreateGUI()
        {
            _serializedObject?.Dispose();
            _serializedObject = new(this);
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            tree.Bind(_serializedObject);
            rootVisualElement.Add(tree);
            LoadFileColorList();

            var colorField = tree.Q<ColorField>("color");
            colorField.SetValueWithoutNotify(ImportWatcher.HighlightColor);
            colorField.RegisterValueChangedCallback(evt => ImportWatcher.HighlightColor = evt.newValue);

            var durationField = tree.Q<SliderInt>("duration");
            durationField.SetValueWithoutNotify(ImportWatcher.Duration);
            durationField.RegisterValueChangedCallback(evt => ImportWatcher.Duration = evt.newValue);

            var maxSessionsField = tree.Q<SliderInt>("maxSessions");
            maxSessionsField.SetValueWithoutNotify(ImportHistoryManager.MaxSessions);
            maxSessionsField.RegisterValueChangedCallback(evt => ImportHistoryManager.MaxSessions = evt.newValue);

            var fileColorsField = tree.Q<ListView>("fileColors");
            var addButton = fileColorsField.Q<Button>("unity-list-view__add-button");
            var removeButton = fileColorsField.Q<Button>("unity-list-view__remove-button");

            addButton.clicked += SaveFileColorList;
            removeButton.clicked += SaveFileColorList;
        }

        private void LoadFileColorList()
        {
            fileColors.Clear();
            fileColors.AddRange(ImportWatcher.ColorList);
            fileColors.Rebuild();
        }

        private void SaveFileColorList()
        {
            ImportWatcher.ColorList = fileColors;
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher Settings", false, Util.Three)]
        internal static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherSettingsWindow>();
            window.titleContent = ToolkitIcons.Content(Title, "settings");
        }
    }
}