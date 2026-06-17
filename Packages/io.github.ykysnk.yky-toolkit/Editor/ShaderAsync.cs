using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    internal static class ShaderAsync
    {
        private const string MenuPath = "Tools/YKYToolkit/Allow Async Compilation";
        private const string EditorKey = "YKYToolkit/AllowAsyncCompilation";

        static ShaderAsync() => ShaderUtil.allowAsyncCompilation = AllowAsyncCompilation;

        internal static bool AllowAsyncCompilation
        {
            get => EditorPrefs.GetBool(EditorKey, true);
            set
            {
                EditorPrefs.SetBool(EditorKey, value);
                ShaderUtil.allowAsyncCompilation = value;
            }
        }

        [MenuItem(MenuPath, false, Util.Twe)]
        private static void Toggle() => AllowAsyncCompilation = !AllowAsyncCompilation;

        [MenuItem(MenuPath, true, Util.Twe)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, AllowAsyncCompilation);
            return true;
        }
    }
}