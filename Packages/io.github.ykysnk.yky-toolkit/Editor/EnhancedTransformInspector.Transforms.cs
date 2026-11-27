using System;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.ykyToolkit.Editor.UIElements;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        /// <summary>
        ///     Whether two sets of Euler angles describe the same rotation. Euler angles are not unique (several sets
        ///     describe one rotation, especially at gimbal lock), so the numbers have to be compared by the rotation
        ///     they produce, the same way Unity's own Transform inspector compares them with its Euler hint.
        /// </summary>
        private static bool IsSameRotation(Vector3 a, Vector3 b) =>
            Mathf.Abs(Quaternion.Dot(Quaternion.Euler(a), Quaternion.Euler(b))) > 0.999999f;

        /// <summary>
        ///     Refreshes a rotation field from the transform without replacing numbers that already describe that
        ///     rotation. Reading the Euler angles back from the quaternion can return a different but equivalent set,
        ///     which would make the value jump right after it was entered.
        /// </summary>
        private static void SetEulerWithoutNotify(Vector3FieldExtra? field, Vector3 euler)
        {
            if (field == null || IsSameRotation(field.value, euler)) return;
            field.SetValueWithoutNotify(euler);
        }

        /// <summary>
        ///     Shows <paramref name="euler" /> as the value that was just entered, unless rounding changed the
        ///     rotation, in which case the transform's own angles are shown instead.
        /// </summary>
        private static void SetEulerShown(Vector3FieldExtra field, Vector3 euler, Vector3 actual) =>
            field.SetValueWithoutNotify(IsSameRotation(euler, actual) ? euler.Clean() : actual.Clean());

        private static void ApplyVector3WhenChanged(Vector3 prev, Vector3 next, Action<Vector3> after)
        {
            var xChanged = !Mathf.Approximately(prev.x, next.x);
            var yChanged = !Mathf.Approximately(prev.y, next.y);
            var zChanged = !Mathf.Approximately(prev.z, next.z);
            if (!xChanged && !yChanged && !zChanged) return;

            var v = prev;
            if (xChanged) v.x = next.x;
            if (yChanged) v.y = next.y;
            if (zChanged) v.z = next.z;
            after(v);
        }

        private void CleanTransforms(float threshold = 0.0001f)
        {
            if (AnimationMode.InAnimationMode()) return;

            foreach (var obj in targets)
            {
                if (obj == null || obj is not Transform t) continue;

                Undo.RecordObject(t, "Clean Transform");

                var exData = EnhancedTransformSettingsAccessor.Get(t);
                var newPosition = t.localPosition.Clean(threshold);
                var newRotation = t.localEulerAngles.Clean(threshold);
                var newScale = t.localScale.Clean(threshold);

                if (exData.positionDecimalPrecision > -1)
                    newPosition = newPosition.Round(exData.positionDecimalPrecision);

                if (exData.rotationDecimalPrecision > -1)
                    newRotation = newRotation.Round(exData.rotationDecimalPrecision);

                if (exData.scaleDecimalPrecision > -1)
                    newScale = newScale.Round(exData.scaleDecimalPrecision);

                // https://issuetracker.unity3d.com/issues/prefab-mode-in-context-in-the-hierarchy-disappears-when-undoing-the-changes
                // Before 6000.0.40f1, undoing the changes in the prefab context view of nested Prefab (Like Variant) makes "Prefab Mode in Context" disappear
                // If also installed Editor Patcher with auto fix prefab override, Clean Transform without check will trigger prefab override fix
                // So only set transform when clean transform is actually changed values
                ApplyVector3WhenChanged(t.localPosition, newPosition, v => t.localPosition = v);
                ApplyVector3WhenChanged(t.localEulerAngles, newRotation, v => t.localEulerAngles = v);
                ApplyVector3WhenChanged(t.localScale, newScale, v => t.localScale = v);
            }
        }

        private void CleanWorldTransforms(float threshold = 0.0001f)
        {
            if (AnimationMode.InAnimationMode()) return;

            foreach (var obj in targets)
            {
                if (obj == null || obj is not Transform t) continue;

                Undo.RecordObject(t, "Clean World Transform");

                var exData = EnhancedTransformSettingsAccessor.Get(t);
                var newPosition = t.position.Clean(threshold);
                var newRotation = t.eulerAngles.Clean(threshold);
                var newScale = t.lossyScale.Clean(threshold);

                if (exData.positionDecimalPrecision > -1)
                    newPosition = newPosition.Round(exData.positionDecimalPrecision);

                if (exData.rotationDecimalPrecision > -1)
                    newRotation = newRotation.Round(exData.rotationDecimalPrecision);

                if (exData.scaleDecimalPrecision > -1)
                    newScale = newScale.Round(exData.scaleDecimalPrecision);

                ApplyVector3WhenChanged(t.position, newPosition, v => t.position = v);
                ApplyVector3WhenChanged(t.eulerAngles, newRotation, v => t.eulerAngles = v);
                ApplyVector3WhenChanged(t.lossyScale, newScale, v => t.SetLossyScale(v));
            }
        }

        private void ApplyToTargets(Action<Transform> action, string undoMessage)
        {
            Undo.RecordObjects(targets, undoMessage);

            foreach (var obj in targets)
                if (obj != null && obj is Transform t)
                    action(t);
        }

        private void ApplyToTargetsInChanged(Vector3 prev, Vector3 next, Func<Transform, Vector3> before,
            Action<Transform, Vector3> after,
            string undoMessage)
        {
            var xChanged = !Mathf.Approximately(prev.x, next.x);
            var yChanged = !Mathf.Approximately(prev.y, next.y);
            var zChanged = !Mathf.Approximately(prev.z, next.z);
            if (!xChanged && !yChanged && !zChanged) return;

            ApplyToTargets(t =>
            {
                var v = before(t);
                var old = v;
                if (xChanged) v.x = next.x;
                if (yChanged) v.y = next.y;
                if (zChanged) v.z = next.z;
                ApplyVector3WhenChanged(old, v, v2 => after(t, v2));
            }, undoMessage);
        }
    }
}