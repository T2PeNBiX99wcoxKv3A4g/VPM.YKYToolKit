using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [PublicAPI]
    public class UnityResourceMonitorRow
    {
        public delegate string ValueHandle(long value);

        public int id;
        public long value;
        public long maxValue;
        public string title;
        public string text;

        public UnityResourceMonitorRow(int id, string title)
        {
            this.id = id;
            value = -1;
            this.title = title;
            text = "- / -";
        }

        public void Update(long newValue, long newNaxValue, ValueHandle valueHandler)
        {
            value = newValue;
            maxValue = newNaxValue;
            text = $"{valueHandler(value)} / {valueHandler(maxValue)}";
        }
    }
}