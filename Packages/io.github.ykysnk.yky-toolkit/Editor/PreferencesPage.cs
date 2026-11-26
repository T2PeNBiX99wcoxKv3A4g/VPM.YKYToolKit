using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
                    tree.UILocalize(false);
                    root.Add(tree);

                    // The Settings window reuses the provider UI, so the scroll view can keep an old offset and hide
                    // the header. Every activation starts at the top.
                    var scroll = tree.Q<ScrollView>();
                    if (scroll != null) scroll.scrollOffset = Vector2.zero;

                    var helper = LocalizationHelper.For(typeof(PreferencesPage).Assembly);
                    var languageHost = tree.Q<VisualElement>("languagePickerHost");
                    if (languageHost != null)
                    {
                        var languages = helper.Languages.ToList();
                        var names = helper.LanguageNames;
                        var selected = Math.Max(0, languages.IndexOf(helper.SelectedLanguage));

                        string Name(string id)
                        {
                            var index = languages.IndexOf(id);
                            return index >= 0 && index < names.Count ? names[index] : id;
                        }

                        var languagePicker = new PopupField<string>("", languages, selected, Name, Name);
                        languagePicker.AddToClassList("yky-language-picker");
                        languagePicker.RegisterValueChangedCallback(evt =>
                        {
                            if (!string.IsNullOrEmpty(evt.newValue)) helper.SelectedLanguage = evt.newValue;
                        });
                        languageHost.Add(languagePicker);
                    }

                    var allowAsyncCompilationToggle = tree.Q<Toggle>("allowAsyncCompilation");
                    allowAsyncCompilationToggle.SetValueWithoutNotify(ShaderAsync.AllowAsyncCompilation);
                    allowAsyncCompilationToggle.RegisterValueChangedCallback(evt =>
                        ShaderAsync.AllowAsyncCompilation = evt.newValue);

                    var deleteSelectedWarnWindowToggle = tree.Q<Toggle>("deleteSelectedWarnWindow");
                    deleteSelectedWarnWindowToggle.SetValueWithoutNotify(NewDelete.ShowWarnWindow);
                    deleteSelectedWarnWindowToggle.RegisterValueChangedCallback(evt =>
                        NewDelete.ShowWarnWindow = evt.newValue);

                    var enableDiscordRichPresenceToggle = tree.Q<Toggle>("enableDiscordRichPresence");
                    enableDiscordRichPresenceToggle.SetValueWithoutNotify(DiscordEditorRPC.EnableDiscordRichPresence);
                    enableDiscordRichPresenceToggle.RegisterValueChangedCallback(evt =>
                        DiscordEditorRPC.EnableDiscordRichPresence = evt.newValue);

                    var deleteHistoryManagerMaxRecordsField = tree.Q<SliderInt>("deleteHistoryManagerMaxRecords");
                    deleteHistoryManagerMaxRecordsField.SetValueWithoutNotify(DeleteHistoryManager.MaxRecords);
                    deleteHistoryManagerMaxRecordsField.RegisterValueChangedCallback(evt =>
                        DeleteHistoryManager.MaxRecords = evt.newValue);

                    tree.Q<Button>("resetGeneral").clicked += () =>
                    {
                        ShaderAsync.ResetSettings();
                        NewDelete.ResetSettings();
                        DiscordEditorRPC.ResetSettings();
                        allowAsyncCompilationToggle.SetValueWithoutNotify(ShaderAsync.AllowAsyncCompilation);
                        deleteSelectedWarnWindowToggle.SetValueWithoutNotify(NewDelete.ShowWarnWindow);
                        enableDiscordRichPresenceToggle.SetValueWithoutNotify(
                            DiscordEditorRPC.EnableDiscordRichPresence);
                    };

                    tree.Q<Button>("resetDeleteHistory").clicked += () =>
                    {
                        DeleteHistoryManager.ResetSettings();
                        deleteHistoryManagerMaxRecordsField.SetValueWithoutNotify(DeleteHistoryManager.MaxRecords);
                    };

                    var importWatcherColorField = tree.Q<ColorField>("importWatcherColor");
                    importWatcherColorField.SetValueWithoutNotify(ImportWatcher.HighlightColor);
                    importWatcherColorField.RegisterValueChangedCallback(evt =>
                        ImportWatcher.HighlightColor = evt.newValue);

                    var importWatcherDurationField = tree.Q<SliderInt>("importWatcherDuration");
                    importWatcherDurationField.SetValueWithoutNotify(ImportWatcher.Duration);
                    importWatcherDurationField.RegisterValueChangedCallback(evt =>
                        ImportWatcher.Duration = evt.newValue);

                    var importWatcherMaxSessionsField = tree.Q<SliderInt>("importWatcherMaxSessionsField");
                    importWatcherMaxSessionsField.SetValueWithoutNotify(ImportHistoryManager.MaxSessions);
                    importWatcherMaxSessionsField.RegisterValueChangedCallback(evt =>
                        ImportHistoryManager.MaxSessions = evt.newValue);

                    var colorCount = tree.Q<Label>("fileColorCount");
                    var newColorList = new List<ImportWatcherFileColor?>(ImportWatcher.ColorList);

                    void UpdateColorCount()
                    {
                        if (colorCount != null) colorCount.text = newColorList.Count.ToString();
                    }

                    var fileColorsField = tree.Q<ListView>("importWatcherFileExtensionColors");
                    fileColorsField.makeItem = () => ImportWatcherFileColorRow.Create(
                        _ => SaveColorList(),
                        value =>
                        {
                            newColorList.Remove(value);
                            SaveColorList();
                            fileColorsField.Rebuild();
                        });
                    fileColorsField.bindItem = (element, index) =>
                    {
                        var value = newColorList.GetValueOrDefault(index) ??
                                    new ImportWatcherFileColor("", ImportWatcherFileColor.DefaultColor);
                        element.userData = value;
                        element.Q<TextField>("fileExtension").SetValueWithoutNotify(value.fileExtension);
                        element.Q<ColorField>("color").SetValueWithoutNotify(value.color);
                    };
                    fileColorsField.itemsSource = newColorList;
                    fileColorsField.Rebuild();
                    UpdateColorCount();

                    // Editing a row only writes the value back; rebuilding here would recycle the row and drop focus
                    // while the user is still typing.
                    void SaveColorList()
                    {
                        ImportWatcher.ColorList = newColorList
                            .Select(value => value ?? new ImportWatcherFileColor("", ImportWatcherFileColor.DefaultColor))
                            .ToList();
                        UpdateColorCount();
                    }

                    void SaveAndRebuildColorList()
                    {
                        SaveColorList();
                        fileColorsField.Rebuild();
                    }

                    fileColorsField.Q<Button>("unity-list-view__add-button").clicked += SaveAndRebuildColorList;
                    fileColorsField.Q<Button>("unity-list-view__remove-button").clicked += SaveAndRebuildColorList;

                    tree.Q<Button>("resetImportWatcher").clicked += () =>
                    {
                        ImportWatcher.ResetSettings();
                        ImportHistoryManager.ResetSettings();
                        importWatcherColorField.SetValueWithoutNotify(ImportWatcher.HighlightColor);
                        importWatcherDurationField.SetValueWithoutNotify(ImportWatcher.Duration);
                        importWatcherMaxSessionsField.SetValueWithoutNotify(ImportHistoryManager.MaxSessions);
                    };

                    fileColorsField.AddFooterReset("resetFileColors", "label.import_watcher.reset_colors".S(), () =>
                    {
                        newColorList.Clear();
                        foreach (var color in ImportWatcherFileColor.DefaultColors)
                            newColorList.Add(color);
                        SaveAndRebuildColorList();
                    });
                }
            };

            return provider;
        }
    }
}
