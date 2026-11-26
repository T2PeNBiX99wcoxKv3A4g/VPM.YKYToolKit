using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        private readonly Dictionary<VisualElement, Action> _pendingExpressions = new();
        private VisualElement? _lastExpressionElement;

        private void ApplyPendingExpression()
        {
            if (_lastExpressionElement == null) return;
            if (_pendingExpressions.TryGetValue(_lastExpressionElement, out var apply)) apply();
        }

        private void ApplyExpressionOnFocusOut(FocusOutEvent evt)
        {
            if (evt.target is VisualElement target &&
                _pendingExpressions.TryGetValue(target, out var apply))
            {
                apply();
                return;
            }

            ApplyPendingExpression();
        }

        private void OnExpressionFocusIn(FocusInEvent evt)
        {
            if (evt.target is VisualElement target && _pendingExpressions.ContainsKey(target))
                _lastExpressionElement = target;
        }

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
            if (vector3Field == null) return;

            ApplyAxis(vector3Field.Q<FloatField>("unity-x-input"), 0);
            ApplyAxis(vector3Field.Q<FloatField>("unity-y-input"), 1);
            ApplyAxis(vector3Field.Q<FloatField>("unity-z-input"), 2);

            void ApplyAxis(FloatField field, int axis)
            {
                var axisGetter = new Func<Transform, float>(t =>
                {
                    var value = getter(t);
                    return axis == 0 ? value.x : axis == 1 ? value.y : value.z;
                });

                var axisSetter = new Action<Transform, float>((t, value) =>
                {
                    var p = getter(t);
                    var old = p;
                    if (axis == 0) p.x = value;
                    else if (axis == 1) p.y = value;
                    else p.z = value;
                    ApplyVector3WhenChanged(old, p, v => setter(t, v));
                });

                // Without a delayed field, Unity parses and applies "+15" as an absolute 15 as soon as it is pasted,
                // then the expression below would add another 15. Delaying until Enter keeps the raw text available.
                field.isDelayed = true;

                bool Apply()
                {
                    if (string.IsNullOrWhiteSpace(field.text)) return false;
                    if (!ApplyParsedInputToAxis(field.text, axisGetter, axisSetter)) return false;

                    onParsed?.Invoke();

                    // Leave the resulting number in the field instead of the expression, so it cannot be applied twice.
                    // The composite field has to be updated as well, or its value would still be the old vector.
                    if (target is Transform theTarget)
                        vector3Field.SetValueWithoutNotify(getter(theTarget).Clean());

                    return true;
                }

                // Expressions used to be polled every second, so "+90" kept adding 90 while the field held the text.
                // They are applied once, when the user commits the field.
                field.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter) return;
                    if (!Apply()) return;

                    evt.StopImmediatePropagation();
                    evt.PreventDefault();
                }, TrickleDown.TrickleDown);

                _pendingExpressions[field] = () => Apply();
                foreach (var element in field.Query<VisualElement>().ToList())
                    _pendingExpressions[element] = () => Apply();
            }
        }
    }
}