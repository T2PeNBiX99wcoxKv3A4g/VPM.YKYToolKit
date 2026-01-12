using io.github.ykysnk.utils.Editor;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class UpmMemoryProfilerInstaller
    {
        [MenuItem("Tools/YKYToolkit/Install Memory Profiler")]
        private static void Install()
        {
            UpmInstaller.Install(new[]
            {
                "com.unity.memoryprofiler"
            });
        }
    }
}