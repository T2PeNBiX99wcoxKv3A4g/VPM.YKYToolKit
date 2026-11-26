using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [PublicAPI]
    public class UnityResourceMonitorRow
    {
        public delegate string ValueHandle(long value);

        public long value;
        public long maxValue;
        public string title;
        public string text;

        public UnityResourceMonitorRow(string title)
        {
            value = -1;
            this.title = title;
            text = "- / -";
        }

        public void Update(long newValue, long newMaxValue, ValueHandle valueHandler)
        {
            value = newValue;
            maxValue = newMaxValue;
            text = $"{valueHandler(value)} / {valueHandler(maxValue)}";
        }

        /// <summary>A value without a maximum, such as a count or a size that has no natural ceiling.</summary>
        public void UpdateAbsolute(long newValue, ValueHandle valueHandler)
        {
            value = newValue;
            maxValue = 0;
            text = valueHandler(value);
        }
    }
}
