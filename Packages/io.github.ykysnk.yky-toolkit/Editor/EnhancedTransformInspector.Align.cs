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
        private static Bounds GetObjectBounds(Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var b = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++)
                    b.Encapsulate(renderers[i].bounds);
                return b;
            }

            var colliders = t.GetComponentsInChildren<Collider>();
            if (colliders.Length <= 0) return new(t.position, Vector3.one);
            {
                var b = colliders[0].bounds;
                for (var i = 1; i < colliders.Length; i++)
                    b.Encapsulate(colliders[i].bounds);
                return b;
            }
        }

        private static void AlignToGround(Transform t, AlignMode mode)
        {
            var selfCol = t.GetComponent<Collider>();
            var origin = t.position + Vector3.up * 1000f;

            // ReSharper disable once Unity.PreferNonAllocApi
            var hits = Physics.RaycastAll(origin, Vector3.down, 5000, ~LayerMask.GetMask("Ignore Raycast"));

            if (hits.Length < 1) return;

            RaycastHit? best = null;

            foreach (var hit in hits)
            {
                if (selfCol != null && hit.collider == selfCol)
                    continue;

                best = hit;
                break;
            }

            if (!best.HasValue)
                return;

            var groundPoint = best.Value.point;
            var b = GetObjectBounds(t);
            var offset = mode switch
            {
                AlignMode.Pivot => 0f,
                AlignMode.Bottom => b.min.y - t.position.y,
                AlignMode.Center => b.center.y - t.position.y,
                _ => 0f
            };

            ApplyVector3WhenChanged(t.position, new(t.position.x, groundPoint.y - offset, t.position.z),
                v => t.position = v);
        }
    }
}
