using io.github.ykysnk.Localization.Editor;
using System;
using System.Globalization;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.ykyToolkit.Editor.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
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

                var exData = EnhancedTransformDatabase.Get(t);
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

                var exData = EnhancedTransformDatabase.Get(t);
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
