using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [PublicAPI]
    public class UnityResourceMonitorRow
    {
        public delegate string ValueHandle(long value);

        public long value;
        public long maxValue;
        public string title;
        public string text;

        protected readonly Process Process;

        protected UnityResourceMonitorRow(string title)
        {
            value = -1;
            this.title = title;
            text = "- / -";
            Process = Process.GetCurrentProcess();
        }

        protected virtual ValueHandle ValueHandler => x => x.ToString();

        public virtual void Update()
        {
        }

        protected void UpdateValue(long newValue, long newNaxValue)
        {
            value = newValue;
            maxValue = newNaxValue;
            text = $"{ValueHandler(value)} / {ValueHandler(maxValue)}";
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        protected static extern int GetGuiResources(IntPtr hProcess, int uiFlags);
#endif
    }
}