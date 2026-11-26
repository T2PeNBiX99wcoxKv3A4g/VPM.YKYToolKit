using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class Util
    {
        internal const string Name = "YKYToolkit";

        private const int Separator = 11;
        internal const int One = 10;
        internal const int Two = One + Separator;
        internal const int Three = Two + Separator;
        internal const int Four = Three + Separator;
        internal const int Five = Four + Separator;

        // First slot after the third group's new lead item (Preferences), and after the fourth group.
        internal const int ThreeAfter = Three + 1;
        internal const int FourAfter = Four + 1;

        internal const int One2 = 1000;
        internal const int Two2 = One2 + Separator;
        internal const int Three2 = Two2 + Separator;

        internal const long StopwatchWaitElapsedMilliseconds = 30;

        // Refs: https://discussions.unity.com/t/how-to-execute-menuitem-for-multiple-objects-once/91492/5
        internal static bool ShouldExecute(MenuCommand menuCommand)
        {
            if (menuCommand.context == null) return true;
            var obj1 = menuCommand.context switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };
            var obj2 = Selection.activeObject switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };
            return obj1 == obj2;
        }
    }
}