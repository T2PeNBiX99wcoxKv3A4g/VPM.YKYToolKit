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
        protected override void OnErrorHandleInspectorGUI()
        {
            var type = ReflectionWrapper.GetType("UnityEditor.TransformInspector, UnityEditor");
            var theTarget = (Transform)target;
            var exData = EnhancedTransformDatabase.Get(theTarget);
            _defaultEditor ??= CreateEditor(targets, type);
            var previousEnabled = GUI.enabled;

            try
            {
                GUI.enabled = !exData.lockTransform;
                _defaultEditor?.OnInspectorGUI();
            }
            finally
            {
                // Restored even when the default inspector throws, otherwise everything drawn later stays greyed out.
                GUI.enabled = previousEnabled;
            }

            EditorGUILayout.TextField("label.enhanced_transform_inspector.hierarchy_path".G(), theTarget.FullName());

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("label.enhanced_transform_inspector.position_decimal_precision".G(),
                GUILayout.Width(100));
            var positionDecimalPrecision = EditorGUILayout.IntSlider(exData.positionDecimalPrecision, -1, 6);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("label.enhanced_transform_inspector.rotation_decimal_precision".G(),
                GUILayout.Width(100));
            var rotationDecimalPrecision = EditorGUILayout.IntSlider(exData.rotationDecimalPrecision, -1, 6);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("label.enhanced_transform_inspector.scale_decimal_precision".G(),
                GUILayout.Width(100));
            var scaleDecimalPrecision = EditorGUILayout.IntSlider(exData.scaleDecimalPrecision, -1, 6);
            EditorGUILayout.EndHorizontal();

            var lockTransform =
                EditorGUILayout.Toggle("label.enhanced_transform_inspector.lock_transform".G(), exData.lockTransform);

            EditorGUILayout.HelpBox("label.enhanced_transform_inspector.imgui".S(), MessageType.Warning);

            if (!EditorGUI.EndChangeCheck()) return;

            exData.positionDecimalPrecision = positionDecimalPrecision;
            exData.rotationDecimalPrecision = rotationDecimalPrecision;
            exData.scaleDecimalPrecision = scaleDecimalPrecision;
            exData.lockTransform = lockTransform;
            EnhancedTransformDatabase.Save();
        }
    }
}
