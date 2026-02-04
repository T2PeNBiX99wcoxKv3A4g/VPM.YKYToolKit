using io.github.ykysnk.utils.Editor;
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
                    importWatcherColorField.value = ImportWatcher.DefaultHighlightColor;
                }
            };

            return provider;
        }
    }
}