using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class UpmMemoryProfilerInstaller
    {
        [MenuItem("Tools/YKYToolkit/Install Memory Profiler")]
        private static void Install()
        {
            _ = new UpmInstaller(new[]
            {
                "com.unity.memoryprofiler"
            }).InstallAsync();
        }
    }
}