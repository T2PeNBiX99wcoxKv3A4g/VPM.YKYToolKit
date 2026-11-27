using System;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Builds one row of the file-extension colour list, so Preferences and the settings window show the same row,
    ///     the same edits and the same context menu.
    /// </summary>
    internal static class ImportWatcherFileColorRow
    {
        private const string RowUxmlGuid = "6bdd0e10034ff1b4281322ac210d8519";

        internal static VisualElement Create(Action<ImportWatcherFileColor> changed,
            Action<ImportWatcherFileColor> removed)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath(RowUxmlGuid));
            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();

            var row = uxml.CloneTree();
            row.UILocalize(false);
            YkyTheme.Apply(row);

            ImportWatcherFileColor? Value() => row.userData as ImportWatcherFileColor;

            row.Q<TextField>("fileExtension").RegisterValueChangedCallback(evt =>
            {
                if (Value() is not { } value) return;
                value.fileExtension = evt.newValue;
                changed(value);
            });
            row.Q<ColorField>("color").RegisterValueChangedCallback(evt =>
            {
                if (Value() is not { } value) return;
                value.color = evt.newValue;
                changed(value);
            });

            row.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (Value() is not { } value) return;

                evt.menu.AppendAction("label.copy".S(), _ =>
                    EditorGUIUtility.systemCopyBuffer = value.fileExtension);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("label.upm_installer_window.remove".S(), _ => removed(value));
            }));

            return row;
        }
    }
}
