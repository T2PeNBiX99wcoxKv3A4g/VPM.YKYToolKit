using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class Util
    {
        internal const string Name = "YKYToolkit";

        private const int Separator = 11;
        internal const int One = 10;
        internal const int Twe = One + Separator;

        // Refs: https://discussions.unity.com/t/how-to-execute-menuitem-for-multiple-objects-once/91492/5
        internal static bool ShouldExecute(MenuCommand menuCommand)
        {
            if (menuCommand.context == null) return true;
            return menuCommand.context == Selection.activeObject;
        }
    }
}