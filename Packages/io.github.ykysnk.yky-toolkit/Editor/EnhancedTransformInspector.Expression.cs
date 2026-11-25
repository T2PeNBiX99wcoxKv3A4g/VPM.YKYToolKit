using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        private bool ApplyParsedInputToAxis(
            string input,
            Func<Transform, float> getter,
            Action<Transform, float> setter)
        {
            var parsed = TransformInputParser.Parse(input);
            if (!parsed.Success)
                return false;

            var count = targets.OfType<Transform>().Count(t => t != null);
            var divisor = Mathf.Max(1, count - 1);
            var index = 0;

            foreach (var obj in targets)
            {
                if (obj == null || obj is not Transform t) continue;

                Undo.RecordObject(t, "Transform Edit");

                var value = getter(t);

                switch (parsed.Mode)
                {
                    case TransformInputMode.Absolute:
                        value = parsed.A;
                        break;

                    case TransformInputMode.Additive:
                        value += parsed.A;
                        break;

                    case TransformInputMode.Multiply:
                        value *= parsed.A;
                        break;

                    case TransformInputMode.Division:
                        value /= parsed.A;
                        break;

                    case TransformInputMode.Linear:
                        value = parsed.A + parsed.B * index;
                        break;

                    case TransformInputMode.Random:
                        value = Random.Range(parsed.A, parsed.B);
                        break;

                    case TransformInputMode.Interpolate:
                        value = Mathf.Lerp(parsed.A, parsed.B, index / (float)divisor);
                        break;

                    case TransformInputMode.InterpolateRev:
                        value = Mathf.Lerp(parsed.A, parsed.B, 1f - index / (float)divisor);
                        break;

                    case TransformInputMode.Clamp:
                        value = Mathf.Clamp(value, parsed.A, parsed.B);
                        break;

                    case TransformInputMode.Mirror:
                        value = parsed.A + parsed.B * (index - (count - 1) * 0.5f);
                        break;

                    case TransformInputMode.Step:
                    {
                        // A step of zero (TT(0,0) or TT(0,0.5)) would throw a DivideByZeroException below.
                        var step = Mathf.Max(1, (int)parsed.B);
                        value = parsed.A + index % step;
                        break;
                    }

                    case TransformInputMode.PingPong:
                        value = Mathf.Lerp(parsed.A, parsed.B, Mathf.PingPong(index, 1f));
                        break;

                    case TransformInputMode.Distance:
                        var center = (targets[0] as Transform)?.position ?? Vector3.zero;
                        var list2 = targets
                            .OfType<Transform>()
                            .Select(t3 => new
                            {
                                t3,
                                dist = Vector3.Distance(t3.position, center)
                            })
                            .OrderBy(e => e.dist)
                            .ToList();

                        var sortedIndex2 = list2.FindIndex(e => e.t3 == t);

                        value = parsed.A + parsed.B * sortedIndex2;
                        break;

                    case TransformInputMode.Angle:
                        var list = targets
                            .OfType<Transform>()
                            .Select(t2 => new
                            {
                                t2,
                                angle = Mathf.Atan2(t2.position.z, t2.position.x) * Mathf.Rad2Deg
                            })
                            .OrderBy(e => e.angle)
                            .ToList();

                        var sortedIndex = list.FindIndex(e => e.t2 == t);

                        value = parsed.A + parsed.B * sortedIndex;
                        break;

                    case TransformInputMode.Noise:
                        var t2 = index * parsed.A;
                        var n = Mathf.PerlinNoise(t2, 0f);
                        value = Mathf.Lerp(parsed.B, parsed.C, n);
                        break;

                    case TransformInputMode.None:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                setter(t, value);
                index++;
            }

            return true;
        }

        private void Vector3FieldApplyParsedInput(Vector3Field? vector3Field, Func<Transform, Vector3> getter,
            Action<Transform, Vector3> setter, Action? onParsed = null)
        {
            var xField = vector3Field.Q<FloatField>("unity-x-input");
            xField.schedule.Execute(() =>
            {
                var parsed = ApplyParsedInputToAxis(
                    xField.text,
                    t => getter(t).x,
                    (t, v) =>
                    {
                        var p = getter(t);
                        var old = p;
                        p.x = v;
                        ApplyVector3WhenChanged(old, p, v2 => setter(t, v2));
                    });
                if (parsed) onParsed?.Invoke();
            }).Every(1000);

            var yField = vector3Field.Q<FloatField>("unity-y-input");
            yField.schedule.Execute(() =>
            {
                var parsed = ApplyParsedInputToAxis(
                    yField.text,
                    t => getter(t).y,
                    (t, v) =>
                    {
                        var p = getter(t);
                        var old = p;
                        p.y = v;
                        ApplyVector3WhenChanged(old, p, v2 => setter(t, v2));
                    });
                if (parsed) onParsed?.Invoke();
            }).Every(1000);

            var zField = vector3Field.Q<FloatField>("unity-z-input");
            zField.schedule.Execute(() =>
            {
                var parsed = ApplyParsedInputToAxis(
                    zField.text,
                    t => getter(t).z,
                    (t, v) =>
                    {
                        var p = getter(t);
                        var old = p;
                        p.z = v;
                        ApplyVector3WhenChanged(old, p, v2 => setter(t, v2));
                    });
                if (parsed) onParsed?.Invoke();
            }).Every(1000);
        }
    }
}