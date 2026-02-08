using System;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.ykyToolkit.Editor.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
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
    public class EnhancedTransformInspector : BasicEditor
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

            rotationField?.SetValueWithoutNotify(theTarget.localEulerAngles.DeltaAngle());
            globalRotationField?.SetValueWithoutNotify(theTarget.eulerAngles.DeltaAngle());
        }

        protected override void OnErrorHandleInspectorGUI()
        {
            var type = ReflectionWrapper.GetType("UnityEditor.TransformInspector, UnityEditor");
            var theTarget = (Transform)target;
            var exData = EnhancedTransformDatabase.Get(theTarget);
            _defaultEditor ??= CreateEditor(targets, type);
            GUI.enabled = !exData.lockTransform;
            _defaultEditor?.OnInspectorGUI();
            GUI.enabled = false;
            EditorGUILayout.TextField("label.enhanced_transform_inspector.hierarchy_path".G(), theTarget.FullName());
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("label.enhanced_transform_inspector.position_decimal_precision".G(),
                GUILayout.Width(100));
            EditorGUILayout.IntSlider(exData.positionDecimalPrecision, -1, 6);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("label.enhanced_transform_inspector.rotation_decimal_precision".G(),
                GUILayout.Width(100));
            EditorGUILayout.IntSlider(exData.rotationDecimalPrecision, -1, 6);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("label.enhanced_transform_inspector.scale_decimal_precision".G(),
                GUILayout.Width(100));
            EditorGUILayout.IntSlider(exData.scaleDecimalPrecision, -1, 6);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Toggle("label.enhanced_transform_inspector.lock_transform".G(), exData.lockTransform);
            GUI.enabled = true;
            EditorGUILayout.HelpBox("label.enhanced_transform_inspector.imgui".S(), MessageType.Warning);
        }

        protected override VisualElement? CreateErrorHandleInspectorGUI()
        {
            var type = ReflectionWrapper.GetType("UnityEditor.TransformInspector, UnityEditor");
            _defaultEditor ??= CreateEditor(targets, type);

            var theTarget = (Transform)target;
            var tree = uxml!.CloneTree();
            _root = tree;
            InternalLocalizationExtensions.Helper.UILocalize(tree);
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
                evt.menu.AppendAction("label.paste".S(), _ => globalPositionField.value = pasteVector3,
                    canBePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));

            positionField.ResetButton.clicked += ResetLocalPosition;
            positionField.RandomButton.clicked += RandomLocalPosition;
            globalPositionField.ResetButton.clicked += ResetGlobalPosition;
            globalPositionField.RandomButton.clicked += RandomGlobalPosition;

            var rotationField = tree.Q<Vector3FieldExtra>("rotation");
            var globalRotationField = tree.Q<Vector3FieldExtra>("globalRotation");

            rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.Clean());
            // Fix the dumb issue of rotation field input
            rotationField.schedule.Execute(() =>
                rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.Clean()));
            globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles.Clean());

            Vector3FieldApplyParsedInput(rotationField, t => t.localEulerAngles,
                (t, newVector) => t.localEulerAngles = newVector);
            Vector3FieldApplyParsedInput(globalRotationField, t => t.eulerAngles,
                (t, newVector) => t.eulerAngles = newVector);

            var rotationEditing = false;

            rotationField.RegisterCallback<FocusInEvent>(_ => rotationEditing = true);
            rotationField.RegisterCallback<FocusOutEvent>(_ => rotationEditing = false);

            rotationField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var prev = evt.previousValue.Clean().DeltaAngle();
                var next = evt.newValue.Clean().DeltaAngle();

                if (exData.rotationDecimalPrecision > -1)
                {
                    prev = prev.Round(exData.rotationDecimalPrecision);
                    next = next.Round(exData.rotationDecimalPrecision);
                }

                if (rotationEditing)
                    ApplyToTargetsInChanged(prev, next, t => t.localEulerAngles.DeltaAngle(),
                        (t, apply) => t.localEulerAngles = apply,
                        "Set Local Rotation");

                CleanTransforms();

                rotationField.SetValueWithoutNotify(next);
                globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles.DeltaAngle());
            });

            var globalRotationEditing = false;

            globalRotationField.RegisterCallback<FocusInEvent>(_ => globalRotationEditing = true);
            globalRotationField.RegisterCallback<FocusOutEvent>(_ => globalRotationEditing = false);

            globalRotationField.schedule.Execute(() =>
            {
                if (theTarget == null || stage == null) return;
                globalRotationField.SetEnabled(
                    stage.IsPartOfPrefabContents(theTarget.gameObject) && !exData.lockTransform);
            }).Every(500);

            globalRotationField.RegisterValueChangedCallback(evt =>
            {
                if (theTarget == null) return;
                if (!stage?.IsPartOfPrefabContents(theTarget.gameObject) ?? false) return;

                var prev = evt.previousValue.Clean().DeltaAngle();
                var next = evt.newValue.Clean().DeltaAngle();

                if (exData.rotationDecimalPrecision > -1)
                {
                    prev = prev.Round(exData.rotationDecimalPrecision);
                    next = next.Round(exData.rotationDecimalPrecision);
                }

                if (globalRotationEditing)
                    ApplyToTargetsInChanged(prev, next, t => t.eulerAngles.DeltaAngle(),
                        (t, apply) => t.eulerAngles = apply,
                        "Set World Rotation");

                CleanTransforms();

                rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.DeltaAngle());
                globalRotationField.SetValueWithoutNotify(next);
            });

            rotationField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("label.copy_property_path".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = "m_LocalRotation");
                evt.menu.AppendAction("label.search_for_same_property".S(), _ =>
                {
                    var rot = theTarget.localEulerAngles;
                    var context = SearchService.CreateContext("scene",
                        $"h:t:Transform #m_LocalRotation=({rot.x},{rot.y},{rot.z})");

                    SearchService.ShowWindow(context);
                });
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("label.copy_euler_angles".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(theTarget.localEulerAngles));
                evt.menu.AppendAction("label.copy_quaternion".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatQuaternionLikeUnity(theTarget.localRotation));

                var canBePasteToVector3 = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                var canBePasteToQuaternion =
                    TryParseUnityQuaternion(EditorGUIUtility.systemCopyBuffer, out var pasteQuaternion);

                evt.menu.AppendAction("label.paste".S(), _ =>
                    {
                        if (canBePasteToVector3)
                            rotationField.value = pasteVector3;
                        else if (canBePasteToQuaternion)
                            theTarget.localRotation = pasteQuaternion;
                    },
                    canBePasteToVector3 || canBePasteToQuaternion
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
            }));

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
                            globalRotationField.value = pasteVector3;
                        else if (canBePasteToQuaternion)
                            theTarget.rotation = pasteQuaternion;
                    },
                    canBePasteToVector3 || canBePasteToQuaternion
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
            }));

            rotationField.ResetButton.clicked += ResetLocalRotation;
            rotationField.RandomButton.clicked += RandomLocalRotation;
            globalRotationField.ResetButton.clicked += ResetGlobalRotation;
            globalRotationField.RandomButton.clicked += RandomGlobalRotation;

            var constrainProportionsScaleToggle = tree.Q<Toggle>("constrainProportionsScale");
            var scaleField = tree.Q<Vector3FieldExtra>("scale");
            var lossyScaleField = tree.Q<Vector3FieldExtra>("lossyScale");

            lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);

            Vector3FieldApplyParsedInput(scaleField, t => t.localScale, (t, newVector) =>
            {
                if (constrainProportionsScaleToggle.value)
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
                    if (constrainProportionsScaleToggle.value)
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

            var scaleXField = scaleField.Q<FloatField>("unity-x-input");
            var scaleYField = scaleField.Q<FloatField>("unity-y-input");
            var scaleZField = scaleField.Q<FloatField>("unity-z-input");

            scaleXField.RegisterValueChangedCallback(evt => OnEditScaleAxis(0, scaleFieldEditing, scaleField, evt));
            scaleYField.RegisterValueChangedCallback(evt => OnEditScaleAxis(1, scaleFieldEditing, scaleField, evt));
            scaleZField.RegisterValueChangedCallback(evt => OnEditScaleAxis(2, scaleFieldEditing, scaleField, evt));

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

            var lossyScaleFieldEditing = false;

            lossyScaleField.RegisterCallback<FocusInEvent>(_ => lossyScaleFieldEditing = true);
            lossyScaleField.RegisterCallback<FocusOutEvent>(_ => lossyScaleFieldEditing = false);

            lossyScaleField.schedule.Execute(() =>
            {
                if (theTarget == null || stage == null) return;
                lossyScaleField.SetEnabled(
                    stage.IsPartOfPrefabContents(theTarget.gameObject) && !exData.lockTransform);
            }).Every(500);

            var lossyScaleXField = lossyScaleField.Q<FloatField>("unity-x-input");
            var lossyScaleYField = lossyScaleField.Q<FloatField>("unity-y-input");
            var lossyScaleZField = lossyScaleField.Q<FloatField>("unity-z-input");

            lossyScaleXField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(0, lossyScaleFieldEditing, lossyScaleField, evt));
            lossyScaleYField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(1, lossyScaleFieldEditing, lossyScaleField, evt));
            lossyScaleZField.RegisterValueChangedCallback(evt =>
                OnEditScaleAxis(2, lossyScaleFieldEditing, lossyScaleField, evt));

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
                evt.menu.AppendAction("label.paste".S(), _ => lossyScaleField.value = pasteVector3,
                    canBePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));

            scaleField.ResetButton.clicked += ResetLocalScale;
            scaleField.RandomButton.clicked += RandomLocalScale;
            lossyScaleField.ResetButton.clicked += ResetGlobalScale;
            lossyScaleField.RandomButton.clicked += RandomGlobalScale;

            scaleField.LinkButton.clicked += ChangeLinkButtonState;
            lossyScaleField.LinkButton.clicked += ChangeLinkButtonState;
            constrainProportionsScaleToggle.RegisterValueChangedCallback(_ => UpdateLinkButtonState());

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

            var copyLocalTransformButton = tree.Q<Button>("copyLocalTransform");
            copyLocalTransformButton.clicked += () =>
            {
                var data = new PRSData(theTarget.localPosition, theTarget.localEulerAngles, theTarget.localScale);
                if (!JsonUtils.TryToJson(data, out var json, out _)) return;
                EditorGUIUtility.systemCopyBuffer = json;
            };

            var pasteLocalTransformButton = tree.Q<Button>("pasteLocalTransform");
            pasteLocalTransformButton.clicked += () =>
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

            var copyGlobalTransformButton = tree.Q<Button>("copyGlobalTransform");
            copyGlobalTransformButton.clicked += () =>
            {
                var data = new PRSData(theTarget.position, theTarget.eulerAngles, theTarget.lossyScale);
                if (!JsonUtils.TryToJson(data, out var json, out _)) return;
                EditorGUIUtility.systemCopyBuffer = json;
            };

            var pasteGlobalTransformButton = tree.Q<Button>("pasteGlobalTransform");
            pasteGlobalTransformButton.clicked += () =>
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

            pasteLocalTransformButton.schedule.Execute(() =>
            {
                var canBePaste = TryParsePRSData(EditorGUIUtility.systemCopyBuffer, out _);
                pasteLocalTransformButton.SetEnabled(canBePaste);
                pasteGlobalTransformButton.SetEnabled(canBePaste);
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
                clearParentButton.SetEnabled(theTarget.parent != null && stage == null);
            });

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

            UpdateLockTransformToggle();

            return tree;

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
                copyLocalTransformButton.SetEnabled(!exData.lockTransform);
                pasteLocalTransformButton.SetEnabled(!exData.lockTransform);
                copyGlobalTransformButton.SetEnabled(!exData.lockTransform);
                pasteGlobalTransformButton.SetEnabled(!exData.lockTransform);
                randomLocalAllButton.SetEnabled(!exData.lockTransform);
                randomGlobalAllButton.SetEnabled(!exData.lockTransform);
            }

            void OnEditScaleAxis(int axis, bool editing, Vector3FieldExtra theScaleField, ChangeEvent<float> evt)
            {
                if (!editing || !constrainProportionsScaleToggle.value) return;

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
            }

            void CleanField()
            {
                positionField.value = positionField.value.Clean();
                rotationField.value = rotationField.value.Clean().DeltaAngle();
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
                if (constrainProportionsScaleToggle.value)
                    constrainProportionsScaleToggle.value = false;
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
                if (constrainProportionsScaleToggle.value)
                    constrainProportionsScaleToggle.value = false;
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
                var randomVector = (Random.insideUnitSphere * 360).Clean().DeltaAngle();
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
                var randomVector = (Random.insideUnitSphere * 360).Clean().DeltaAngle();
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

            void UpdateLinkButtonState()
            {
                scaleField.SetLinked(constrainProportionsScaleToggle.value);
                lossyScaleField.SetLinked(constrainProportionsScaleToggle.value);
            }

            void ChangeLinkButtonState()
            {
                constrainProportionsScaleToggle.value = !constrainProportionsScaleToggle.value;
            }
        }

        private static string FormatVector3LikeUnity(Vector3 v) =>
            $"Vector3({FormatFloat(v.x)},{FormatFloat(v.y)},{FormatFloat(v.z)})";

        private static string FormatQuaternionLikeUnity(Quaternion q) =>
            $"Quaternion({FormatFloat(q.x)},{FormatFloat(q.y)},{FormatFloat(q.z)},{FormatFloat(q.w)})";

        private static string FormatFloat(float f) => Mathf.Approximately(f, Mathf.Round(f))
            ? Mathf.RoundToInt(f).ToString()
            : f.ToString("0.########");

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

            try
            {
                var x = float.Parse(parts[0]);
                var y = float.Parse(parts[1]);
                var z = float.Parse(parts[2]);
                result = new(x, y, z);
                return true;
            }
            catch
            {
                return false;
            }
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

            try
            {
                var x = float.Parse(parts[0]);
                var y = float.Parse(parts[1]);
                var z = float.Parse(parts[2]);
                var w = float.Parse(parts[3]);
                result = new(x, y, z, w);
                return true;
            }
            catch
            {
                return false;
            }
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
            foreach (var obj in targets)
            {
                if (obj is not Transform t) continue;

                Undo.RecordObject(t, "Clean Transform");

                var exData = EnhancedTransformDatabase.Get(t);
                var newPosition = t.localPosition.Clean(threshold);
                var newRotation = t.localEulerAngles.Clean(threshold).DeltaAngle();
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
            foreach (var obj in targets)
            {
                if (obj is not Transform t) continue;

                Undo.RecordObject(t, "Clean World Transform");

                var exData = EnhancedTransformDatabase.Get(t);
                var newPosition = t.position.Clean(threshold);
                var newRotation = t.eulerAngles.Clean(threshold).DeltaAngle();
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
                if (obj is Transform t)
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

        private bool ApplyParsedInputToAxis(
            string input,
            Func<Transform, float> getter,
            Action<Transform, float> setter)
        {
            var parsed = TransformInputParser.Parse(input);
            if (!parsed.Success)
                return false;

            var count = targets.Length;
            var index = 0;

            foreach (var obj in targets)
            {
                if (obj is not Transform t) continue;

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
                        value = Mathf.Lerp(parsed.A, parsed.B, index / (float)(count - 1));
                        break;

                    case TransformInputMode.InterpolateRev:
                        value = Mathf.Lerp(parsed.A, parsed.B, 1f - index / (float)(count - 1));
                        break;

                    case TransformInputMode.Clamp:
                        value = Mathf.Clamp(value, parsed.A, parsed.B);
                        break;

                    case TransformInputMode.Mirror:
                        value = parsed.A + parsed.B * (index - (count - 1) * 0.5f);
                        break;

                    case TransformInputMode.Step:
                        value = parsed.A + index % (int)parsed.B;
                        break;

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
                                dist = Vector3.Distance(t.position, center)
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
                                angle = Mathf.Atan2(t.position.z, t.position.x) * Mathf.Rad2Deg
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