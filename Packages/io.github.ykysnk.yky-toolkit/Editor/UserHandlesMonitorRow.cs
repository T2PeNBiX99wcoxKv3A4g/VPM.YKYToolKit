namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class UserHandlesMonitorRow : UnityResourceMonitorRow
    {
        public UserHandlesMonitorRow() : base("USER Handles")
        {
        }

        protected override ValueHandle ValueHandler => x => x > 0 ? $"{x}" : "N/A";

        public override void Update()
        {
#if UNITY_EDITOR_WIN
            var userHandlesValue = GetGuiResources(Process.Handle, 0);
#else
            var userHandlesValue = -1;
#endif

            UpdateValue(userHandlesValue, 10000);
        }
    }
}