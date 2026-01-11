using System;
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

        public UnityResourceMonitorRow(string title2, long maxValue2)
        {
            value = -1;
            maxValue = maxValue2;
            title = title2;
            text = "- / -";
        }

        public void Update(long newValue, ValueHandle valueHandler)
        {
            value = newValue;
            text = $"{valueHandler(value)} / {valueHandler(maxValue)}";
        }
    }
}