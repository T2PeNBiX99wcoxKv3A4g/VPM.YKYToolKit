namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class GdiHandlesMonitorRow : UnityResourceMonitorRow
    {
        public GdiHandlesMonitorRow() : base("GDI Handles")
        {
        }

        protected override ValueHandle ValueHandler => x => x > 0 ? $"{x}" : "N/A";

        public override void Update()
        {
#if UNITY_EDITOR_WIN
            var gdiHandlesValue = GetGuiResources(Process.Handle, 1);
#else
            var gdiHandlesValue = -1;
#endif

            UpdateValue(gdiHandlesValue, 10000);
        }
    }
}