using UnityEngine;
using UnityEngine.Profiling;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class GCAllocatedMonitorRow : UnityResourceMonitorRow
    {
        public GCAllocatedMonitorRow() : base("GC Allocated")
        {
        }

        protected override ValueHandle ValueHandler => x => $"{x} MB";

        public override void Update()
        {
            UpdateValue(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024, SystemInfo.systemMemorySize);
        }
    }
}