using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    internal static class ShaderAsync
    {
        private const string MenuPath = "Tools/YKYToolkit/Allow Async Compilation";

        static ShaderAsync() => ShaderUtil.allowAsyncCompilation = ShowWarnWindow;

        private static bool ShowWarnWindow
        {
            set
            {
                EditorPrefs.SetBool("YKYToolkit/AllowAsyncCompilation", value);
                ShaderUtil.allowAsyncCompilation = value;
            }
            get => EditorPrefs.GetBool("YKYToolkit/AllowAsyncCompilation", true);
        }

        [MenuItem(MenuPath, false, Util.Twe)]
        private static void Toggle() => ShowWarnWindow = !ShowWarnWindow;

        [MenuItem(MenuPath, true, Util.Twe)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, ShowWarnWindow);
            return true;
        }
    }
}