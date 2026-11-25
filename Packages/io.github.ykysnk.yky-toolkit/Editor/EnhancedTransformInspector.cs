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
    public enum AlignMode
    {
        Pivot,
        Bottom,
        Center
    }

    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    public partial class EnhancedTransformInspector : BasicEditor
    {
        [SerializeField] private VisualTreeAsset? uxml;

        private UnityEditor.Editor? _defaultEditor;
        private VisualElement? _root;

        private static bool IsDefaultUIExpanded
        {
            get => EditorPrefs.GetBool("YKYToolkit/TransformDefaultUIExpanded");
            set => EditorPrefs.SetBool("YKYToolkit/TransformDefaultUIExpanded", value);
        }

        private static bool IsHelpExpanded
        {
            get => EditorPrefs.GetBool("YKYToolkit/TransformHelpExpanded");
            set => EditorPrefs.SetBool("YKYToolkit/TransformHelpExpanded", value);
        }

        private static AlignMode AlignModeSave
        {
            get => (AlignMode)EditorPrefs.GetInt("YKYToolkit/TransformAlignMode", 1);
            set => EditorPrefs.SetInt("YKYToolkit/TransformAlignMode", (int)value);
        }

        protected override void OnEnable()
        {
            Undo.undoRedoPerformed += RefreshRotationFields;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= RefreshRotationFields;
        }

        private void OnDestroy()
        {
            if (_defaultEditor != null)
                DestroyImmediate(_defaultEditor);
        }

        private void RefreshRotationFields()
        {
            if (target == null || _root == null) return;
            var theTarget = (Transform)target;

            var tree = _root;
            var rotationField = tree.Q<Vector3FieldExtra>("rotation");
            var globalRotationField = tree.Q<Vector3FieldExtra>("globalRotation");

            rotationField?.SetValueWithoutNotify(theTarget.localEulerAngles);
            globalRotationField?.SetValueWithoutNotify(theTarget.eulerAngles);
        }


        protected override VisualElement CreateErrorHandleInspectorGUI()
        {
            var type = ReflectionWrapper.GetType("UnityEditor.TransformInspector, UnityEditor");
            _defaultEditor ??= CreateEditor(targets, type);

            var theTarget = (Transform)target;
            var positionProperty = serializedObject.FindProperty("m_LocalPosition");
            var rotationProperty = serializedObject.FindProperty("m_LocalRotation");
            var eulerAnglesHintProperty = serializedObject.FindProperty("m_LocalEulerAnglesHint");
            var scaleProperty = serializedObject.FindProperty("m_LocalScale");
            var tree = uxml!.CloneTree();
            _root = tree;
            tree.UILocalize();
            tree.Bind(serializedObject);

            var exData = EnhancedTransformDatabase.Get(theTarget);
            var stage = PrefabStageUtility.GetCurrentPrefabStage();

            var defaultUIFoldOut = tree.Q<Foldout>("defaultGUIFoldout");
            defaultUIFoldOut.SetValueWithoutNotify(IsDefaultUIExpanded);
            defaultUIFoldOut.RegisterValueChangedCallback(evt => IsDefaultUIExpanded = evt.newValue);

            var defaultGUI = tree.Q<IMGUIContainer>("defaultGUI");
            defaultGUI.onGUIHandler = () =>
            {
                GUI.enabled = !exData.lockTransform;
                _defaultEditor?.OnInspectorGUI();
            };

            var positionField = tree.Q<Vector3FieldExtra>("position");
            var globalPositionField = tree.Q<Vector3FieldExtra>("globalPosition");

            globalPositionField.SetValueWithoutNotify(theTarget.position);

            Vector3FieldApplyParsedInput(positionField, t => t.localPosition,
                (t, newVector) => t.localPosition = newVector);
            Vector3FieldApplyParsedInput(globalPositionField, t => t.position, (t, newVector) => t.position = newVector);

            positionField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var clearVector = evt.newValue.Clean();

                if (exData.positionDecimalPrecision > -1)
                    clearVector = clearVector.Round(exData.positionDecimalPrecision);

                CleanTransforms();

                positionField.SetValueWithoutNotify(clearVector);
                globalPositionField.SetValueWithoutNotify(theTarget.position);
            });

            positionField.CopyButton.clicked += () =>
                EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(positionField.value);
            positionField.PasteButton.clicked += () =>
            {
                if (!TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3)) return;
                positionField.value = pasteVector3;
            };
            positionField.schedule.Execute(() =>
                    positionField.PasteButton.SetEnabled(!exData.lockTransform && TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out _)))
                .Every(100);

            var globalPositionFieldEditing = false;

            globalPositionField.RegisterCallback<FocusInEvent>(_ => globalPositionFieldEditing = true);
            globalPositionField.RegisterCallback<FocusOutEvent>(_ => globalPositionFieldEditing = false);

            globalPositionField.schedule.Execute(() =>
            {
                if (theTarget == null || stage == null) return;
                var shouldEnable = stage.IsPartOfPrefabContents(theTarget.gameObject);

                if (theTarget.parent != null)
                    shouldEnable = stage.mode == PrefabStage.Mode.InContext &&
                                   stage.IsPartOfPrefabContents(theTarget.parent.gameObject);

                globalPositionField.SetEnabled(shouldEnable && !exData.lockTransform);
            }).Every(500);

            globalPositionField.schedule.Execute(() =>
            {
                if (theTarget == null) return;
                globalPositionField.SetValueWithoutNotify(theTarget.position.Clean());
            }).Every(1000);

            globalPositionField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var prev = evt.previousValue.Clean();
                var next = evt.newValue.Clean();

                if (exData.positionDecimalPrecision > -1)
                {
                    prev = prev.Round(exData.positionDecimalPrecision);
                    next = next.Round(exData.positionDecimalPrecision);
                }

                if (globalPositionFieldEditing)
                    ApplyToTargetsInChanged(prev, next, t => t.position, (t, apply) => t.position = apply,
                        "Set World Position");

                CleanTransforms();

                positionField.SetValueWithoutNotify(theTarget.localPosition);
                globalPositionField.SetValueWithoutNotify(next);
            });

            globalPositionField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("label.copy".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(globalPositionField.value));
                var canBePaste = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                evt.menu.AppendAction("label.paste".S(), _ =>
                    {
                        // The field only applies to the targets while it has focus, which a menu click does not give it.
                        globalPositionFieldEditing = true;
                        globalPositionField.value = pasteVector3;
                        globalPositionFieldEditing = false;
                    },
                    canBePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));

            globalPositionField.CopyButton.clicked += () =>
                EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(globalPositionField.value);
            globalPositionField.PasteButton.clicked += () =>
            {
                if (!TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3)) return;
                globalPositionFieldEditing = true;
                globalPositionField.value = pasteVector3;
                globalPositionFieldEditing = false;
            };
            globalPositionField.schedule.Execute(() =>
                globalPositionField.PasteButton.SetEnabled(!exData.lockTransform && TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer,
                    out _))).Every(100);

            positionField.ResetButton.clicked += ResetLocalPosition;
            positionField.RandomButton.clicked += RandomLocalPosition;
            globalPositionField.ResetButton.clicked += ResetGlobalPosition;
            globalPositionField.RandomButton.clicked += RandomGlobalPosition;

            var rotationField = tree.Q<Vector3FieldExtra>("rotation");
            var globalRotationField = tree.Q<Vector3FieldExtra>("globalRotation");

            rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.Clean());
            globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles.Clean());

            Vector3FieldApplyParsedInput(rotationField, t => t.localEulerAngles,
                (t, newVector) => t.localEulerAngles = newVector);
            Vector3FieldApplyParsedInput(globalRotationField, t => t.eulerAngles,
                (t, newVector) => t.eulerAngles = newVector);

            var rotationEditing = false;

            rotationField.RegisterCallback<FocusInEvent>(_ => rotationEditing = true);
            rotationField.RegisterCallback<FocusOutEvent>(_ => rotationEditing = false);

            // Fix the dumb issue of rotation field input
            rotationField.schedule.Execute(UpdateLocalRotation).ExecuteLater(100);
            rotationField.schedule.Execute(UpdateLocalRotation).Every(1000);

            rotationField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var prev = evt.previousValue.Clean();
                var next = evt.newValue.Clean();

                if (exData.rotationDecimalPrecision > -1)
                {
                    prev = prev.Round(exData.rotationDecimalPrecision);
                    next = next.Round(exData.rotationDecimalPrecision);
                }

                if (rotationEditing)
                    ApplyToTargetsInChanged(prev, next, t => t.localEulerAngles, (t, apply) => t.localEulerAngles = apply,
                        "Set Local Rotation");

                CleanTransforms();

                rotationField.SetValueWithoutNotify(next);
                globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles);
            });

            rotationField.CopyButton.clicked += () =>
                EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(rotationField.value);
            rotationField.PasteButton.clicked += () =>
            {
                if (!TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3)) return;
                rotationField.value = pasteVector3;
            };
            rotationField.schedule.Execute(() =>
                    rotationField.PasteButton.SetEnabled(!exData.lockTransform && TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out _)))
                .Every(100);

            var globalRotationEditing = false;

            globalRotationField.RegisterCallback<FocusInEvent>(_ => globalRotationEditing = true);
            globalRotationField.RegisterCallback<FocusOutEvent>(_ => globalRotationEditing = false);

            globalRotationField.schedule.Execute(() =>
            {
                if (theTarget == null || stage == null) return;
                globalRotationField.SetEnabled(
                    stage.IsPartOfPrefabContents(theTarget.gameObject) && !exData.lockTransform);
            }).Every(500);

            globalRotationField.schedule.Execute(() =>
            {
                if (theTarget == null) return;
                globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles.Clean());
            }).Every(1000);

            globalRotationField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var prev = evt.previousValue.Clean();
                var next = evt.newValue.Clean();

                if (exData.rotationDecimalPrecision > -1)
                {
                    prev = prev.Round(exData.rotationDecimalPrecision);
                    next = next.Round(exData.rotationDecimalPrecision);
                }

                if (globalRotationEditing)
                    ApplyToTargetsInChanged(prev, next, t => t.eulerAngles, (t, apply) => t.eulerAngles = apply,
                        "Set World Rotation");

                CleanTransforms();

                rotationField.SetValueWithoutNotify(theTarget.localEulerAngles);
                globalRotationField.SetValueWithoutNotify(next);
            });

            globalRotationField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("label.copy_euler_angles".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(globalRotationField.value));
                evt.menu.AppendAction("label.copy_quaternion".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatQuaternionLikeUnity(theTarget.rotation));

                var canBePasteToVector3 = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                var canBePasteToQuaternion =
                    TryParseUnityQuaternion(EditorGUIUtility.systemCopyBuffer, out var pasteQuaternion);

                evt.menu.AppendAction("label.paste".S(), _ =>
                    {
                        if (canBePasteToVector3)
                        {
                            globalRotationEditing = true;
                            globalRotationField.value = pasteVector3;
                            globalRotationEditing = false;
                        }
                        else if (canBePasteToQuaternion)
                            foreach (var transform in targets.OfType<Transform>())
                            {
                                if (!transform) continue;
                                Undo.RecordObject(transform, "Set World Rotation");
                                transform.rotation = pasteQuaternion;
                            }
                    },
                    canBePasteToVector3 || canBePasteToQuaternion
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
            }));

            globalRotationField.CopyButton.clicked += () =>
                EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(globalRotationField.value);
            globalRotationField.PasteButton.clicked += () =>
            {
                if (!TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3)) return;
                globalRotationEditing = true;
                globalRotationField.value = pasteVector3;
                globalRotationEditing = false;
            };
            globalRotationField.schedule.Execute(() =>
                    globalRotationField.PasteButton.SetEnabled(!exData.lockTransform && TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer,
                        out _)))
                .Every(100);

            rotationField.ResetButton.clicked += ResetLocalRotation;
            rotationField.RandomButton.clicked += RandomLocalRotation;
            globalRotationField.ResetButton.clicked += ResetGlobalRotation;
            globalRotationField.RandomButton.clicked += RandomGlobalRotation;

            var scaleField = tree.Q<Vector3FieldExtra>("scale");
            var lossyScaleField = tree.Q<Vector3FieldExtra>("lossyScale");

            lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);

            Vector3FieldApplyParsedInput(scaleField, t => t.localScale, (t, newVector) =>
            {
                if (scaleField.LinkButton.value)
                {
                    var prev = t.localScale;
                    var ratio = 1f;
                    if (!Mathf.Approximately(newVector.x, prev.x))
                        ratio = !Mathf.Approximately(prev.x, 0f) ? newVector.x / prev.x : 1f;
                    else if (!Mathf.Approximately(newVector.y, prev.y))
                        ratio = !Mathf.Approximately(prev.y, 0f) ? newVector.y / prev.y : 1f;
                    else if (!Mathf.Approximately(newVector.z, prev.z))
                        ratio = !Mathf.Approximately(prev.z, 0f) ? newVector.z / prev.z : 1f;

                    if (!Mathf.Approximately(ratio, 1f))
                        t.localScale = prev * ratio;
                }
                else
                    t.localScale = newVector;
            }, () =>
            {
                scaleField.SetValueWithoutNotify(theTarget.localScale);
                lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);
            });
            Vector3FieldApplyParsedInput(lossyScaleField, t => t.lossyScale,
                (t, newVector) =>
                {
                    if (scaleField.LinkButton.value)
                    {
                        var prev = t.lossyScale;
                        var ratio = 1f;
                        if (!Mathf.Approximately(newVector.x, prev.x))
                            ratio = !Mathf.Approximately(prev.x, 0f) ? newVector.x / prev.x : 1f;
                        else if (!Mathf.Approximately(newVector.y, prev.y))
                            ratio = !Mathf.Approximately(prev.y, 0f) ? newVector.y / prev.y : 1f;
                        else if (!Mathf.Approximately(newVector.z, prev.z))
                            ratio = !Mathf.Approximately(prev.z, 0f) ? newVector.z / prev.z : 1f;

                        if (!Mathf.Approximately(ratio, 1f))
                            t.SetLossyScale(prev * ratio);
                    }
                    else
                        t.SetLossyScale(newVector);
                }, () =>
                {
                    scaleField.SetValueWithoutNotify(theTarget.localScale);
                    lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);
                });

            var scaleFieldEditing = false;

            scaleField.RegisterCallback<FocusInEvent>(_ => scaleFieldEditing = true);
            scaleField.RegisterCallback<FocusOutEvent>(_ => scaleFieldEditing = false);

            scaleField.schedule.Execute(() =>
            {
                if (theTarget == null) return;
                scaleField.SetValueWithoutNotify(theTarget.localScale.Clean());
            }).Every(1000);

            var scaleXField = scaleField.Q<FloatField>("unity-x-input");
            var scaleYField = scaleField.Q<FloatField>("unity-y-input");
            var scaleZField = scaleField.Q<FloatField>("unity-z-input");

            scaleXField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(0, scaleFieldEditing, scaleField, evt, false));
            scaleYField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(1, scaleFieldEditing, scaleField, evt, false));
            scaleZField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(2, scaleFieldEditing, scaleField, evt, false));

            scaleField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var clear = evt.newValue.Clean();

                if (exData.scaleDecimalPrecision > -1)
                    clear = clear.Round(exData.scaleDecimalPrecision);

                CleanTransforms();

                scaleField.SetValueWithoutNotify(clear);
                lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);
            });

            scaleField.CopyButton.clicked += () =>
                EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(scaleField.value);
            scaleField.PasteButton.clicked += () =>
            {
                if (!TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3)) return;
                scaleField.value = pasteVector3;
            };
            scaleField.schedule.Execute(() =>
                    scaleField.PasteButton.SetEnabled(!exData.lockTransform && TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out _)))
                .Every(100);

            var lossyScaleFieldEditing = false;

            lossyScaleField.RegisterCallback<FocusInEvent>(_ => lossyScaleFieldEditing = true);
            lossyScaleField.RegisterCallback<FocusOutEvent>(_ => lossyScaleFieldEditing = false);

            lossyScaleField.schedule.Execute(() =>
            {
                if (theTarget == null || stage == null) return;
                lossyScaleField.SetEnabled(
                    stage.IsPartOfPrefabContents(theTarget.gameObject) && !exData.lockTransform);
            }).Every(500);

            lossyScaleField.schedule.Execute(() =>
            {
                if (theTarget == null) return;
                lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale.Clean());
            }).Every(1000);

            var lossyScaleXField = lossyScaleField.Q<FloatField>("unity-x-input");
            var lossyScaleYField = lossyScaleField.Q<FloatField>("unity-y-input");
            var lossyScaleZField = lossyScaleField.Q<FloatField>("unity-z-input");

            lossyScaleXField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(0, lossyScaleFieldEditing, lossyScaleField, evt, true));
            lossyScaleYField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(1, lossyScaleFieldEditing, lossyScaleField, evt, true));
            lossyScaleZField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(2, lossyScaleFieldEditing, lossyScaleField, evt, true));

            lossyScaleField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var prev = evt.previousValue.Clean();
                var next = evt.newValue.Clean();

                if (exData.scaleDecimalPrecision > -1)
                {
                    prev = prev.Round(exData.scaleDecimalPrecision);
                    next = next.Round(exData.scaleDecimalPrecision);
                }

                if (lossyScaleFieldEditing)
                    ApplyToTargetsInChanged(prev, next, t => t.lossyScale, (t, apply) => t.SetLossyScale(apply),
                        "Set World Lossy Scale");

                CleanTransforms();

                scaleField.SetValueWithoutNotify(theTarget.localScale);
                lossyScaleField.SetValueWithoutNotify(next);
            });

            lossyScaleField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("label.copy".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(lossyScaleField.value));
                var canBePaste = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                evt.menu.AppendAction("label.paste".S(), _ =>
                    {
                        // The field only applies to the targets while it has focus, which a menu click does not give it.
                        lossyScaleFieldEditing = true;
                        lossyScaleField.value = pasteVector3;
                        lossyScaleFieldEditing = false;
                    },
                    canBePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));

            lossyScaleField.CopyButton.clicked += () =>
                EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(lossyScaleField.value);
            lossyScaleField.PasteButton.clicked += () =>
            {
                if (!TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3)) return;
                lossyScaleFieldEditing = true;
                lossyScaleField.value = pasteVector3;
                lossyScaleFieldEditing = false;
            };
            lossyScaleField.schedule.Execute(() =>
                    lossyScaleField.PasteButton.SetEnabled(!exData.lockTransform && TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer,
                        out _)))
                .Every(100);

            scaleField.ResetButton.clicked += ResetLocalScale;
            scaleField.RandomButton.clicked += RandomLocalScale;
            lossyScaleField.ResetButton.clicked += ResetGlobalScale;
            lossyScaleField.RandomButton.clicked += RandomGlobalScale;

            var resetLocalAllButton = tree.Q<IconButton>("resetLocalAll");
            resetLocalAllButton.style.backgroundImage = Vector3FieldExtra.ResetIcon;
            resetLocalAllButton.clicked += () =>
            {
                ResetLocalPosition();
                ResetLocalRotation();
                ResetLocalScale();
            };

            var resetGlobalAllButton = tree.Q<IconButton>("resetGlobalAll");
            resetGlobalAllButton.style.backgroundImage = Vector3FieldExtra.ResetIcon;
            resetGlobalAllButton.clicked += () =>
            {
                ResetGlobalPosition();
                ResetGlobalRotation();
                ResetGlobalScale();
            };

            var randomLocalAllButton = tree.Q<IconButton>("randomLocalAll");
            randomLocalAllButton.style.backgroundImage = Vector3FieldExtra.RandomIcon;
            randomLocalAllButton.clicked += () =>
            {
                RandomLocalPosition();
                RandomLocalRotation();
                RandomLocalScale();
            };

            var randomGlobalAllButton = tree.Q<IconButton>("randomGlobalAll");
            randomGlobalAllButton.style.backgroundImage = Vector3FieldExtra.RandomIcon;
            randomGlobalAllButton.clicked += () =>
            {
                RandomGlobalPosition();
                RandomGlobalRotation();
                RandomGlobalScale();
            };

            var copyLocalAllButton = tree.Q<IconButton>("copyLocalAll");
            copyLocalAllButton.clicked += () =>
            {
                if (theTarget == null) return;
                var data = new PRSData(theTarget.localPosition, theTarget.localEulerAngles, theTarget.localScale);
                if (!JsonUtils.TryToJson(data, out var json, out _)) return;
                EditorGUIUtility.systemCopyBuffer = json;
            };

            var pasteLocalAllButton = tree.Q<IconButton>("pasteLocalAll");
            pasteLocalAllButton.clicked += () =>
            {
                if (!TryParsePRSData(EditorGUIUtility.systemCopyBuffer, out var data)) return;
                ApplyToTargets(t =>
                {
                    ApplyVector3WhenChanged(t.localPosition, data.position, v => t.localPosition = v);
                    ApplyVector3WhenChanged(t.localEulerAngles, data.eulerAngles, v => t.localEulerAngles = v);
                    ApplyVector3WhenChanged(t.localScale, data.scale, v => t.localScale = v);
                }, "Paste Local Transform");
                positionField.value = data.position;
                rotationField.value = data.eulerAngles;
                scaleField.value = data.scale;
            };

            var copyGlobalAllButton = tree.Q<IconButton>("copyGlobalAll");
            copyGlobalAllButton.clicked += () =>
            {
                if (theTarget == null) return;
                var data = new PRSData(theTarget.position, theTarget.eulerAngles, theTarget.lossyScale);
                if (!JsonUtils.TryToJson(data, out var json, out _)) return;
                EditorGUIUtility.systemCopyBuffer = json;
            };

            var pasteGlobalAllButton = tree.Q<IconButton>("pasteGlobalAll");
            pasteGlobalAllButton.clicked += () =>
            {
                if (!TryParsePRSData(EditorGUIUtility.systemCopyBuffer, out var data)) return;
                ApplyToTargets(t =>
                {
                    ApplyVector3WhenChanged(t.position, data.position, v => t.position = v);
                    ApplyVector3WhenChanged(t.eulerAngles, data.eulerAngles, v => t.eulerAngles = v);
                    ApplyVector3WhenChanged(t.lossyScale, data.scale, t.SetLossyScale);
                }, "Paste Global Transform");
                globalPositionField.value = data.position;
                globalRotationField.value = data.eulerAngles;
                lossyScaleField.value = data.scale;
            };

            pasteLocalAllButton.schedule.Execute(() =>
            {
                var canBePaste = !exData.lockTransform && TryParsePRSData(EditorGUIUtility.systemCopyBuffer, out _);
                pasteLocalAllButton.SetEnabled(canBePaste);
                pasteGlobalAllButton.SetEnabled(canBePaste);
            }).Every(100);

            var hierarchyPathField = tree.Q<TextField>("hierarchyPath");
            hierarchyPathField.schedule.Execute(() =>
                {
                    if (theTarget == null) return;
                    hierarchyPathField.SetValueWithoutNotify(theTarget.FullName());
                })
                .Every(1000);

            hierarchyPathField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("label.copy".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = hierarchyPathField.value);
            }));

            var positionDecimalField = tree.Q<SliderInt>("positionDecimal");
            positionDecimalField.value = exData.positionDecimalPrecision;
            positionDecimalField.RegisterValueChangedCallback(evt =>
            {
                exData.positionDecimalPrecision = evt.newValue;
                EnhancedTransformDatabase.Save();
            });

            var rotationDecimalField = tree.Q<SliderInt>("rotationDecimal");
            rotationDecimalField.value = exData.rotationDecimalPrecision;
            rotationDecimalField.RegisterValueChangedCallback(evt =>
            {
                exData.rotationDecimalPrecision = evt.newValue;
                EnhancedTransformDatabase.Save();
            });

            var scaleDecimalField = tree.Q<SliderInt>("scaleDecimal");
            scaleDecimalField.value = exData.scaleDecimalPrecision;
            scaleDecimalField.RegisterValueChangedCallback(evt =>
            {
                exData.scaleDecimalPrecision = evt.newValue;
                EnhancedTransformDatabase.Save();
            });

            var alignToParentButton = tree.Q<Button>("alignToParent");
            alignToParentButton.clicked += () =>
            {
                ApplyToTargets(t =>
                {
                    if (t.parent == null) return;
                    t.position = t.parent.position.Clean();
                }, "Align to Parent");
                CleanWorldTransforms();
                CleanField();
            };

            var alignModeField = tree.Q<EnumField>("alignMode");
            alignModeField.SetValueWithoutNotify(AlignModeSave);
            alignModeField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is not AlignMode mode) return;
                AlignModeSave = mode;
            });

            var alignToGroundButton = tree.Q<Button>("alignToGround");
            alignToGroundButton.clicked += () =>
            {
                ApplyToTargets(t => AlignToGround(t, AlignModeSave), "Align to Ground");
                CleanWorldTransforms();
                CleanField();
            };

            var clearParentButton = tree.Q<Button>("clearParent");
            clearParentButton.clicked += () =>
            {
                if (stage != null) return;
                ApplyToTargets(t => t.SetParent(null, true), "Clear Parent");
            };
            clearParentButton.schedule.Execute(() =>
            {
                if (theTarget == null) return;
                clearParentButton.SetEnabled(theTarget.parent != null && stage == null && !exData.lockTransform);
            }).Every(500);

            var helpFoldout = tree.Q<Foldout>("helpFoldout");
            helpFoldout.value = IsHelpExpanded;
            helpFoldout.RegisterValueChangedCallback(evt => IsHelpExpanded = evt.newValue);

            var helpText = tree.Q<Label>("helpText");
            helpText.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("label.copy".S(), _ => EditorGUIUtility.systemCopyBuffer = helpText.text)));

            var lockTransformToggle = tree.Q<Toggle>("lockTransform");
            lockTransformToggle.value = exData.lockTransform;
            lockTransformToggle.RegisterValueChangedCallback(evt =>
            {
                exData.lockTransform = evt.newValue;
                UpdateLockTransformToggle();
                EnhancedTransformDatabase.Save();
            });

            // Reused by the once-per-second override check; creating a SerializedObject there every second was the
            // expensive part.
            SerializedObject? prefabTargetSerializedObject = null;
            SerializedProperty? prefabTargetPos = null;
            SerializedProperty? prefabTargetRot = null;
            SerializedProperty? prefabTargetEulerHint = null;
            SerializedProperty? prefabTargetScale = null;

            void EnsurePrefabProperties()
            {
                if (prefabTargetSerializedObject != null || theTarget == null) return;

                var prefabTarget = PrefabUtility.GetCorrespondingObjectFromSource(theTarget);
                if (prefabTarget == null) return;

                prefabTargetSerializedObject = new SerializedObject(prefabTarget);
                prefabTargetPos = prefabTargetSerializedObject.FindProperty("m_LocalPosition");
                prefabTargetRot = prefabTargetSerializedObject.FindProperty("m_LocalRotation");
                prefabTargetEulerHint = prefabTargetSerializedObject.FindProperty("m_LocalEulerAnglesHint");
                prefabTargetScale = prefabTargetSerializedObject.FindProperty("m_LocalScale");
            }

            tree.schedule.Execute(ClearOverride).Every(1000);
            ClearOverride();
            UpdateLockTransformToggle();

            return tree;

            void UpdateLocalRotation()
            {
                if (theTarget == null) return;
                rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.Clean());
            }

            void ClearOverride()
            {
                if (theTarget == null) return;
                if (AnimationMode.InAnimationMode()) return;

                EnsurePrefabProperties();
                if (prefabTargetSerializedObject == null) return;

                var isChanged = false;

                if (positionProperty != null && prefabTargetPos != null && positionProperty.prefabOverride &&
                    positionProperty.vector3Value.IsNearly(prefabTargetPos.vector3Value))
                {
                    PrefabUtility.RevertPropertyOverride(positionProperty, InteractionMode.AutomatedAction);
                    isChanged = true;
                }

                if (rotationProperty != null && prefabTargetRot != null && rotationProperty.prefabOverride &&
                    Quaternion.Dot(rotationProperty.quaternionValue, prefabTargetRot.quaternionValue) > 0.999999f)
                {
                    PrefabUtility.RevertPropertyOverride(rotationProperty, InteractionMode.AutomatedAction);
                    isChanged = true;
                }

                if (eulerAnglesHintProperty != null && prefabTargetEulerHint != null &&
                    eulerAnglesHintProperty.prefabOverride &&
                    eulerAnglesHintProperty.vector3Value.IsNearly(prefabTargetEulerHint.vector3Value))
                {
                    PrefabUtility.RevertPropertyOverride(eulerAnglesHintProperty, InteractionMode.AutomatedAction);
                    isChanged = true;
                }

                if (scaleProperty != null && prefabTargetScale != null && scaleProperty.prefabOverride &&
                    scaleProperty.vector3Value.IsNearly(prefabTargetScale.vector3Value))
                {
                    PrefabUtility.RevertPropertyOverride(scaleProperty, InteractionMode.AutomatedAction);
                    isChanged = true;
                }

                if (!isChanged) return;
                serializedObject.Update();
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            void UpdateLockTransformToggle()
            {
                positionField.SetEnabled(!exData.lockTransform);
                rotationField.SetEnabled(!exData.lockTransform);
                scaleField.SetEnabled(!exData.lockTransform);
                globalPositionField.SetEnabled(!exData.lockTransform);
                globalRotationField.SetEnabled(!exData.lockTransform);
                lossyScaleField.SetEnabled(!exData.lockTransform);
                alignToGroundButton.SetEnabled(!exData.lockTransform);
                alignToParentButton.SetEnabled(!exData.lockTransform);
                clearParentButton.SetEnabled(!exData.lockTransform);
                resetLocalAllButton.SetEnabled(!exData.lockTransform);
                resetGlobalAllButton.SetEnabled(!exData.lockTransform);
                copyLocalAllButton.SetEnabled(!exData.lockTransform);
                pasteLocalAllButton.SetEnabled(!exData.lockTransform);
                copyGlobalAllButton.SetEnabled(!exData.lockTransform);
                pasteGlobalAllButton.SetEnabled(!exData.lockTransform);
                randomLocalAllButton.SetEnabled(!exData.lockTransform);
                randomGlobalAllButton.SetEnabled(!exData.lockTransform);
            }

            void OnEditScaleAxis(int axis, bool editing, Vector3FieldExtra theScaleField, ChangeEvent<float> evt,
                bool isGlobal)
            {
                if (!editing || !scaleField.LinkButton.value) return;

                var prev = evt.previousValue;
                var next = evt.newValue;
                var ratio = !Mathf.Approximately(prev, 0f) ? next / prev : 1f;

                if (Mathf.Approximately(ratio, 1f)) return;

                var scale = theScaleField.value;

                switch (axis)
                {
                    case 0:
                        scale.y *= ratio;
                        scale.z *= ratio;
                        break;
                    case 1:
                        scale.x *= ratio;
                        scale.z *= ratio;
                        break;
                    case 2:
                        scale.x *= ratio;
                        scale.y *= ratio;
                        break;
                }

                theScaleField.value = scale;

                if (isGlobal)
                    theTarget.SetLossyScale(scale);
                else
                    theTarget.localScale = scale;
            }

            void CleanField()
            {
                positionField.value = positionField.value.Clean();
                rotationField.value = rotationField.value.Clean();
                scaleField.value = scaleField.value.Clean();
            }

            void ResetLocalPosition()
            {
                ApplyToTargets(t => ApplyVector3WhenChanged(t.localPosition, Vector3.zero, v => t.localPosition = v),
                    "Reset Local Position");
                positionField.value = Vector3.zero;
            }

            void ResetLocalRotation()
            {
                rotationEditing = true;
                ApplyToTargets(
                    t => ApplyVector3WhenChanged(t.localEulerAngles, Vector3.zero, v => t.localEulerAngles = v),
                    "Reset Local Rotation");
                rotationField.value = Vector3.zero;
                rotationField.schedule.Execute(() => rotationEditing = false);
            }

            void ResetLocalScale()
            {
                if (scaleField.LinkButton.value)
                    scaleField.LinkButton.value = false;
                ApplyToTargets(t => ApplyVector3WhenChanged(t.localScale, Vector3.one, v => t.localScale = v),
                    "Reset Local Scale");
                scaleField.value = Vector3.one;
            }

            void ResetGlobalPosition()
            {
                ApplyToTargets(t => ApplyVector3WhenChanged(t.position, Vector3.zero, v => t.position = v),
                    "Reset World Position");
                globalPositionField.value = Vector3.zero;
            }

            void ResetGlobalRotation()
            {
                rotationEditing = true;
                ApplyToTargets(t => ApplyVector3WhenChanged(t.eulerAngles, Vector3.zero, v => t.eulerAngles = v),
                    "Reset World Rotation");
                globalRotationField.value = Vector3.zero;
                globalPositionField.schedule.Execute(() => rotationEditing = false);
            }

            void ResetGlobalScale()
            {
                if (lossyScaleField.LinkButton.value)
                    lossyScaleField.LinkButton.value = false;
                ApplyToTargets(t => ApplyVector3WhenChanged(t.lossyScale, Vector3.one, t.SetLossyScale),
                    "Reset World Scale");
                lossyScaleField.value = Vector3.one;
            }

            void RandomLocalPosition()
            {
                var randomVector = (Random.insideUnitSphere * 10).Clean();
                ApplyToTargets(t => t.localPosition += randomVector, "Random Local Position");
                positionField.value += randomVector;
            }

            void RandomLocalRotation()
            {
                rotationEditing = true;
                var randomVector = (Random.insideUnitSphere * 360).Clean();
                ApplyToTargets(t => t.localEulerAngles = randomVector, "Random Local Rotation");
                rotationField.value = randomVector;
                rotationField.schedule.Execute(() => rotationEditing = false);
            }

            void RandomLocalScale()
            {
                var randomVector = (Random.insideUnitSphere * 2).Clean();
                ApplyToTargets(t => t.localScale += randomVector, "Random Local Scale");
                scaleField.value += randomVector;
            }

            void RandomGlobalPosition()
            {
                var randomVector = (Random.insideUnitSphere * 10).Clean();
                ApplyToTargets(t => t.position += randomVector, "Random World Position");
                globalPositionField.value += randomVector;
            }

            void RandomGlobalRotation()
            {
                rotationEditing = true;
                var randomVector = (Random.insideUnitSphere * 360).Clean();
                ApplyToTargets(t => t.eulerAngles = randomVector, "Random World Rotation");
                globalRotationField.value = randomVector;
                globalPositionField.schedule.Execute(() => rotationEditing = false);
            }

            void RandomGlobalScale()
            {
                var randomVector = (Random.insideUnitSphere * 2).Clean();
                ApplyToTargets(t => t.SetLossyScale(t.lossyScale + randomVector), "Random World Scale");
                lossyScaleField.value += randomVector;
            }
        }

    }
}