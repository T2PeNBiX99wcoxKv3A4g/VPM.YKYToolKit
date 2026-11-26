using System.Collections.Generic;
using io.github.ykysnk.Localization.Editor;
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

        private Label? _fileColorCount;
        private ListView? _fileColorsField;

        private void CreateGUI()
        {
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            rootVisualElement.Add(tree);

            var colorField = tree.Q<ColorField>("color");
            colorField.SetValueWithoutNotify(ImportWatcher.HighlightColor);
            colorField.RegisterValueChangedCallback(evt => ImportWatcher.HighlightColor = evt.newValue);

            var durationField = tree.Q<SliderInt>("duration");
            durationField.SetValueWithoutNotify(ImportWatcher.Duration);
            durationField.RegisterValueChangedCallback(evt => ImportWatcher.Duration = evt.newValue);

            var maxSessionsField = tree.Q<SliderInt>("maxSessions");
            maxSessionsField.SetValueWithoutNotify(ImportHistoryManager.MaxSessions);
            maxSessionsField.RegisterValueChangedCallback(evt => ImportHistoryManager.MaxSessions = evt.newValue);

            tree.Q<Button>("resetSettings").clicked += () =>
            {
                ImportWatcher.ResetSettings();
                ImportHistoryManager.ResetSettings();
                colorField.SetValueWithoutNotify(ImportWatcher.HighlightColor);
                durationField.SetValueWithoutNotify(ImportWatcher.Duration);
                maxSessionsField.SetValueWithoutNotify(ImportHistoryManager.MaxSessions);
            };

            _fileColorCount = tree.Q<Label>("fileColorCount");
            var fileColorsField = tree.Q<ListView>("fileColors");
            _fileColorsField = fileColorsField;
            fileColorsField.makeItem = () => ImportWatcherFileColorRow.Create(
                _ => SaveFileColorList(),
                value =>
                {
                    fileColors.Remove(value);
                    fileColorsField.Rebuild();
                    SaveFileColorList();
                });
            fileColorsField.bindItem = (element, index) =>
            {
                if (index < 0 || index >= fileColors.Count) return;
                var value = fileColors[index];
                element.userData = value;
                element.Q<TextField>("fileExtension").SetValueWithoutNotify(value.fileExtension);
                element.Q<ColorField>("color").SetValueWithoutNotify(value.color);
            };
            LoadFileColorList();
            fileColorsField.itemsSource = fileColors;

            var addButton = fileColorsField.Q<Button>("unity-list-view__add-button");
            var removeButton = fileColorsField.Q<Button>("unity-list-view__remove-button");
            addButton.clicked += SaveFileColorList;
            removeButton.clicked += SaveFileColorList;

            fileColorsField.AddFooterReset("resetFileColors", "label.import_watcher.reset_colors".S(), () =>
            {
                fileColors.Clear();
                fileColors.AddRange(ImportWatcherFileColor.DefaultColors);
                fileColorsField.Rebuild();
                SaveFileColorList();
            });
        }

        private void LoadFileColorList()
        {
            fileColors.Clear();
            fileColors.AddRange(ImportWatcher.ColorList);
            fileColors.Rebuild();
            UpdateColorCount();
        }

        private void SaveFileColorList()
        {
            for (var i = 0; i < fileColors.Count; i++)
                fileColors[i] ??= new ImportWatcherFileColor("", ImportWatcherFileColor.DefaultColor);

            ImportWatcher.ColorList = fileColors;
            UpdateColorCount();
        }

        private void UpdateColorCount()
        {
            if (_fileColorCount != null) _fileColorCount.text = fileColors.Count.ToString();
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher Settings", false, Util.Three)]
        internal static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherSettingsWindow>();
            window.titleContent = ToolkitIcons.Content(Title, "settings");
        }
    }
}
