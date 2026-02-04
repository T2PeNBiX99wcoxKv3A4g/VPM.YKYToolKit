using System.Collections.Generic;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
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
            fileColors.Rebuild();

            var colorField = tree.Q<ColorField>("color");
            colorField.value = ColorUtility.TryParseHtmlString(EditorPrefs.GetString(ImportWatcher.ImportHighlightColor),
                out var color)
                ? color
                : ImportWatcher.DefaultHighlightColor;
            colorField.RegisterValueChangedCallback(evt =>
                EditorPrefs.SetString(ImportWatcher.ImportHighlightColor,
                    $"#{ColorUtility.ToHtmlStringRGBA(evt.newValue)}"));

            var doubleField = tree.Q<DoubleField>("duration");
            doubleField.value =
                EditorPrefs.GetFloat(ImportWatcher.ImportHighlightDuration, (float)ImportWatcher.DefaultDuration);
            doubleField.RegisterValueChangedCallback(evt =>
                EditorPrefs.SetFloat(ImportWatcher.ImportHighlightDuration, (float)evt.newValue));

            var fileColorsField = tree.Q<ListView>("fileColors");
            var addButton = fileColorsField.Q<Button>("unity-list-view__add-button");
            var removeButton = fileColorsField.Q<Button>("unity-list-view__remove-button");

            addButton.clicked += SaveFileColorList;
            removeButton.clicked += SaveFileColorList;
        }

        private void LoadFileColorList()
        {
            var json = EditorPrefs.GetString(ImportWatcher.ImportHighlightFileColor);

            if (!JsonUtils.TryFromJson<ListWrapper<ImportWatcherFileColor>>(json, out var colors, out _))
            {
                fileColors.Clear();
                fileColors.AddRange(ImportWatcherFileColor.DefaultColors);
                return;
            }

            fileColors.Clear();
            fileColors.AddRange(colors!);
        }

        private void SaveFileColorList()
        {
            if (!JsonUtils.TryToJson(Wrapper.Create(fileColors), out var json, out _)) return;
            EditorPrefs.SetString(ImportWatcher.ImportHighlightFileColor, json);
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherWindow>();
            window.titleContent = EditorGUIUtils.IconContent(Title, "package manager");
        }
    }
}