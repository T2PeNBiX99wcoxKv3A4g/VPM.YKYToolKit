using System.Collections.Generic;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class ImportWatcherSettingsWindow : EditorWindow
    {
        private const string Title = "Import Watcher Settings";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<ImportWatcherFileColor> fileColors = new();

        private void OnDestroy()
        {
            fileColors.Rebuild();
            SaveFileColorList();
        }

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree);
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            LoadFileColorList();

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
            window.titleContent = EditorGUIUtils.IconContent(Title, "settings");
        }
    }
}