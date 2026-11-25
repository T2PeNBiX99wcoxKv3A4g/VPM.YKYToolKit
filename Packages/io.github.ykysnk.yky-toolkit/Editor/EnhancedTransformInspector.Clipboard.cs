using System.Globalization;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        private static string FormatVector3LikeUnity(Vector3 v) =>
            $"Vector3({FormatFloat(v.x)},{FormatFloat(v.y)},{FormatFloat(v.z)})";

        private static string FormatQuaternionLikeUnity(Quaternion q) =>
            $"Quaternion({FormatFloat(q.x)},{FormatFloat(q.y)},{FormatFloat(q.z)},{FormatFloat(q.w)})";

        private static string FormatFloat(float f) => Mathf.Approximately(f, Mathf.Round(f))
            ? Mathf.RoundToInt(f).ToString(CultureInfo.InvariantCulture)
            : f.ToString("0.########", CultureInfo.InvariantCulture);

        private static bool TryParseUnityVector3(string s, out Vector3 result)
        {
            result = default;

            s = s.Trim();

            if (!s.StartsWith("Vector3(") || !s.EndsWith(")"))
                return false;

            s = s.MiddlePath('(', ')')!;

            var parts = s.Split(',');
            if (parts.Length != 3)
                return false;

            // The clipboard is shared with Unity's own fields, which always use the invariant format.
            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
                !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
                return false;

            result = new(x, y, z);
            return true;
        }

        private static bool TryParseUnityQuaternion(string s, out Quaternion result)
        {
            result = default;

            s = s.Trim();

            if (!s.StartsWith("Quaternion(") || !s.EndsWith(")"))
                return false;

            s = s.MiddlePath('(', ')')!;

            var parts = s.Split(',');
            if (parts.Length != 4)
                return false;

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
                !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z) ||
                !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var w))
                return false;

            result = new(x, y, z, w);
            return true;
        }

        private static bool TryParsePRSData(string json, out PRSData data)
        {
            data = default;

            if (string.IsNullOrWhiteSpace(json))
                return false;

            if (!JsonUtils.TryFromJson(json, out data, out _)) return false;
            if (data.scale == default && data.position == default && data.eulerAngles == default)
                return json.Contains("position") || json.Contains("eulerAngles") || json.Contains("scale");
            return true;
        }
    }
}