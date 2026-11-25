using io.github.ykysnk.Localization.Editor;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class PreferencesPage
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyPreferencesProvider()
        {
            var provider = new SettingsProvider("Preferences/YKYToolkit", SettingsScope.User)
            {
                label = "YKY Toolkit",

                activateHandler = (searchContext, root) =>
                {
                    var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        AssetDatabase.GUIDToAssetPath("a15da8d2cfd690e428145ca535509163"));

                    if (uxml == null)
                    {
                        root.Add(BasicEditor.CreateUxmlImportErrorUI());
                        return;
                    }

                    var tree = uxml.CloneTree();
                    tree.UILocalize();
                    root.Add(tree);

                    var allowAsyncCompilationToggle = tree.Q<Toggle>("allowAsyncCompilation");
                    allowAsyncCompilationToggle.value = ShaderAsync.AllowAsyncCompilation;
                    allowAsyncCompilationToggle.RegisterValueChangedCallback(evt =>
                        ShaderAsync.AllowAsyncCompilation = evt.newValue);

                    var deleteSelectedWarnWindowToggle = tree.Q<Toggle>("deleteSelectedWarnWindow");
                    deleteSelectedWarnWindowToggle.value = NewDelete.ShowWarnWindow;
                    deleteSelectedWarnWindowToggle.RegisterValueChangedCallback(evt =>
                        NewDelete.ShowWarnWindow = evt.newValue);

                    var enableDiscordRichPresenceToggle = tree.Q<Toggle>("enableDiscordRichPresence");
                    enableDiscordRichPresenceToggle.value = DiscordEditorRPC.EnableDiscordRichPresence;
                    enableDiscordRichPresenceToggle.RegisterValueChangedCallback(evt =>
                        DiscordEditorRPC.EnableDiscordRichPresence = evt.newValue);

                    var deleteHistoryManagerMaxRecordsField = tree.Q<SliderInt>("deleteHistoryManagerMaxRecords");
                    deleteHistoryManagerMaxRecordsField.value = DeleteHistoryManager.MaxRecords;
                    deleteHistoryManagerMaxRecordsField.RegisterValueChangedCallback(evt =>
                        DeleteHistoryManager.MaxRecords = evt.newValue);

                    var importWatcherColorField = tree.Q<ColorField>("importWatcherColor");
                    importWatcherColorField.value = ImportWatcher.HighlightColor;
                    importWatcherColorField.RegisterValueChangedCallback(evt =>
                        ImportWatcher.HighlightColor = evt.newValue);

                    var importWatcherDurationField = tree.Q<SliderInt>("importWatcherDuration");
                    importWatcherDurationField.value = ImportWatcher.Duration;
                    importWatcherDurationField.RegisterValueChangedCallback(evt => ImportWatcher.Duration = evt.newValue);

                    var importWatcherMaxSessionsField = tree.Q<SliderInt>("importWatcherMaxSessionsField");
                    importWatcherMaxSessionsField.value = ImportHistoryManager.MaxSessions;
                    importWatcherMaxSessionsField.RegisterValueChangedCallback(evt =>
                        ImportHistoryManager.MaxSessions = evt.newValue);

                    var importWatcherFileExtensionColorsListView = tree.Q<ListView>("importWatcherFileExtensionColors");
                    importWatcherFileExtensionColorsListView.LocalizeEmptyLabel();
                    var newColorList = new List<ImportWatcherFileColor?>(ImportWatcher.ColorList);
                    importWatcherFileExtensionColorsListView.itemsSource = newColorList;
                    importWatcherFileExtensionColorsListView.makeItem = () =>
                    {
                        var uxml2 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                            AssetDatabase.GUIDToAssetPath("6bdd0e10034ff1b4281322ac210d8519"));

                        if (uxml2 == null)
                            return BasicEditor.CreateUxmlImportErrorUI();

                        var tree2 = uxml2.CloneTree();
                        tree2.UILocalize(false);

                        // Registered once here: bindItem runs every time a row is reused, and registering the callbacks
                        // there piled up one more handler per scroll.
                        tree2.Q<TextField>("fileExtension").RegisterValueChangedCallback(evt =>
                        {
                            if (tree2.userData is not ImportWatcherFileColor value) return;
                            value.fileExtension = evt.newValue;
                            SaveColorList();
                        });
                        tree2.Q<ColorField>("color").RegisterValueChangedCallback(evt =>
                        {
                            if (tree2.userData is not ImportWatcherFileColor value) return;
                            value.color = evt.newValue;
                            SaveColorList();
                        });

                        return tree2;
                    };
                    importWatcherFileExtensionColorsListView.bindItem = (element, index) =>
                    {
                        var value = newColorList.GetValueOrDefault(index) ??
                                    new ImportWatcherFileColor("", ImportWatcherFileColor.DefaultColor);
                        element.userData = value;
                        element.Q<TextField>("fileExtension").SetValueWithoutNotify(value.fileExtension);
                        element.Q<ColorField>("color").SetValueWithoutNotify(value.color);
                    };

                    var addButton = importWatcherFileExtensionColorsListView.Q<Button>("unity-list-view__add-button");
                    var removeButton =
                        importWatcherFileExtensionColorsListView.Q<Button>("unity-list-view__remove-button");

                    // The built-in add button appends a null for a class element, so the list is normalized before it
                    // is written back.
                    void SaveColorList()
                    {
                        ImportWatcher.ColorList = newColorList
                            .Select(value => value ?? new ImportWatcherFileColor("", ImportWatcherFileColor.DefaultColor))
                            .ToList();
                        importWatcherFileExtensionColorsListView.Rebuild();
                    }

                    addButton.clicked += SaveColorList;
                    removeButton.clicked += SaveColorList;
                }
            };

            return provider;
        }
    }
}