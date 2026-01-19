using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    internal static class ShaderAsync
    {
        private const string MenuPath = "Tools/YKYToolkit/Allow Async Compilation";

        static ShaderAsync() => ShaderUtil.allowAsyncCompilation = AllowAsyncCompilation;

        private static bool AllowAsyncCompilation
        {
            set
            {
                EditorPrefs.SetBool("YKYToolkit/AllowAsyncCompilation", value);
                ShaderUtil.allowAsyncCompilation = value;
            }
            get => EditorPrefs.GetBool("YKYToolkit/AllowAsyncCompilation", true);
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