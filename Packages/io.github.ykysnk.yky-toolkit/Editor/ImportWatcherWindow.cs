using io.github.ykysnk.utils.Editor;
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

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree);
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);

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
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherWindow>();
            window.titleContent = EditorGUIUtils.IconContent(Title, "package manager");
        }
    }
}