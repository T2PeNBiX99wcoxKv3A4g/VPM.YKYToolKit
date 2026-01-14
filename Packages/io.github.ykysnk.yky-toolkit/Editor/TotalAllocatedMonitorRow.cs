using UnityEngine;
using UnityEngine.Profiling;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class TotalAllocatedMonitorRow : UnityResourceMonitorRow
    {
        public TotalAllocatedMonitorRow() : base("Total Allocated")
        {
        }

        protected override ValueHandle ValueHandler => x => $"{x} MB";

        public override void Update()
        {
            UpdateValue(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024, SystemInfo.systemMemorySize);
        }
    }
}