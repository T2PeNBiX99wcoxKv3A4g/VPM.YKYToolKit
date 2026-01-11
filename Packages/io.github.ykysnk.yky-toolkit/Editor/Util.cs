using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class Util
    {
        internal const string Name = "YKYToolkit";

        internal const int ObjectAndTagChangePriority = 10;

        // Refs: https://discussions.unity.com/t/how-to-execute-menuitem-for-multiple-objects-once/91492/5
        internal static bool ShouldExecute(MenuCommand menuCommand)
        {
            if (menuCommand.context == null) return true;
            return menuCommand.context == Selection.activeObject;
        }
    }
}