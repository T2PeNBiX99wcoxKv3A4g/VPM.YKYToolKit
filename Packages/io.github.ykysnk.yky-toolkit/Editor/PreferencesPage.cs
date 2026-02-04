using System.Collections.Generic;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public static class PreferencesPage
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
                    InternalLocalizationExtensions.Helper.UILocalize(tree);
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

                    var importWatcherColorField = tree.Q<ColorField>("importWatcherColor");
                    importWatcherColorField.value = ImportWatcher.HighlightColor;
                    importWatcherColorField.RegisterValueChangedCallback(evt =>
                        ImportWatcher.HighlightColor = evt.newValue);

                    var importWatcherDurationField = tree.Q<DoubleField>("importWatcherDuration");
                    importWatcherDurationField.value = ImportWatcher.Duration;
                    importWatcherDurationField.RegisterValueChangedCallback(evt => ImportWatcher.Duration = evt.newValue);

                    var importWatcherFileExtensionColorsListView = tree.Q<ListView>("importWatcherFileExtensionColors");
                    var newColorList = new List<ImportWatcherFileColor>(ImportWatcher.ColorList);
                    importWatcherFileExtensionColorsListView.itemsSource = newColorList;
                    importWatcherFileExtensionColorsListView.makeItem = () =>
                    {
                        var uxml2 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                            AssetDatabase.GUIDToAssetPath("6bdd0e10034ff1b4281322ac210d8519"));

                        if (uxml2 == null)
                            return BasicEditor.CreateUxmlImportErrorUI();

                        var tree2 = uxml2.CloneTree();
                        InternalLocalizationExtensions.Helper.UILocalize(tree, false);
                        return tree2;
                    };
                    importWatcherFileExtensionColorsListView.bindItem = (element, index) =>
                    {
                        var value = newColorList.GetValueOrDefault(index) ?? new("", ImportWatcherFileColor.DefaultColor);
                        var fileExtensionField = element.Q<TextField>("fileExtension");
                        fileExtensionField.value = value.fileExtension;
                        fileExtensionField.RegisterValueChangedCallback(evt =>
                        {
                            value.fileExtension = evt.newValue;
                            ImportWatcher.ColorList = newColorList;
                        });
                        var colorField = element.Q<ColorField>("color");
                        colorField.value = value.color;
                        colorField.RegisterValueChangedCallback(evt =>
                        {
                            value.color = evt.newValue;
                            ImportWatcher.ColorList = newColorList;
                        });
                    };

                    var addButton = importWatcherFileExtensionColorsListView.Q<Button>("unity-list-view__add-button");
                    var removeButton =
                        importWatcherFileExtensionColorsListView.Q<Button>("unity-list-view__remove-button");

                    addButton.clicked += () => ImportWatcher.ColorList = newColorList;
                    removeButton.clicked += () => ImportWatcher.ColorList = newColorList;
                    importWatcherFileExtensionColorsListView.SetEnabled(false);
                }
            };

            return provider;
        }
    }
}