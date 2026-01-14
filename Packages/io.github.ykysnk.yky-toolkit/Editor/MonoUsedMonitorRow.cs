using UnityEngine.Profiling;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class MonoUsedMonitorRow : UnityResourceMonitorRow
    {
        public MonoUsedMonitorRow() : base("Mono Used")
        {
        }

        protected override ValueHandle ValueHandler => x => $"{x} MB";

        public override void Update()
        {
            UpdateValue(Profiler.GetMonoUsedSizeLong() / 1024 / 1024, Profiler.GetMonoHeapSizeLong() / 1024 / 1024);
        }
    }
}