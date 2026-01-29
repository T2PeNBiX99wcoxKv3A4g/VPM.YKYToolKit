using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ObjectField = UnityEditor.UIElements.ObjectField;
using Random = UnityEngine.Random;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    public class EnhancedTransformInspector : BasicEditor
    {
        private static bool _locked;

        private static Vector3 _copyPos;
        private static Vector3 _copyRot;
        private static Vector3 _copyScale;
        private static bool _hasCopy;

        [SerializeField] private VisualTreeAsset? uxml;
        private UnityEditor.Editor? _defaultEditor;

        private static bool IsDefaultUIExpanded
        {
            get => EditorPrefs.GetBool("YKYToolkit/TransformDefaultUIExpanded");
            set => EditorPrefs.SetBool("YKYToolkit/TransformDefaultUIExpanded", value);
        }

        private static bool IsChildrenListExpanded
        {
            get => EditorPrefs.GetBool("YKYToolkit/TransformChildrenListExpanded");
            set => EditorPrefs.SetBool("YKYToolkit/TransformChildrenListExpanded", value);
        }

        private void OnDestroy()
        {
            if (_defaultEditor != null)
                DestroyImmediate(_defaultEditor);
        }

        protected override VisualElement? CreateErrorHandleInspectorGUI()
        {
            var type = ReflectionWrapper.GetType("UnityEditor.TransformInspector, UnityEditor");
            _defaultEditor ??= CreateEditor(targets, type);

            var theTarget = (Transform)target;
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree);
            tree.Bind(serializedObject);

            var defaultUIFoldOut = tree.Q<Foldout>("defaultGUIFoldout");
            defaultUIFoldOut.value = IsDefaultUIExpanded;
            defaultUIFoldOut.RegisterValueChangedCallback(evt => IsDefaultUIExpanded = evt.newValue);

            var defaultGUI = tree.Q<IMGUIContainer>("defaultGUI");
            defaultGUI.onGUIHandler = () => _defaultEditor?.OnInspectorGUI();

            var positionField = tree.Q<Vector3Field>("position");
            var globalPositionField = tree.Q<Vector3Field>("globalPosition");

            globalPositionField.value = theTarget.position;

            Vector3FieldApplyParsedInput(positionField, t => t.localPosition,
                (t, newVector) => t.localPosition = newVector);
            Vector3FieldApplyParsedInput(globalPositionField, t => t.position, (t, newVector) => t.position = newVector);

            positionField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean();

                CleanTransforms();

                positionField.SetValueWithoutNotify(clearVector);
                globalPositionField.SetValueWithoutNotify(theTarget.position);
            });

            globalPositionField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean();

                CleanTransforms();

                positionField.SetValueWithoutNotify(theTarget.localPosition);
                globalPositionField.SetValueWithoutNotify(clearVector);
            });

            globalPositionField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(globalPositionField.value));
                var canBePaste = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                evt.menu.AppendAction("Paste", _ => globalPositionField.value = pasteVector3,
                    canBePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));

            var rotationField = tree.Q<Vector3Field>("rotation");
            var globalRotationField = tree.Q<Vector3Field>("globalRotation");

            globalRotationField.value = theTarget.eulerAngles;

            Vector3FieldApplyParsedInput(rotationField, t => t.localEulerAngles,
                (t, newVector) => t.localEulerAngles = newVector);
            Vector3FieldApplyParsedInput(globalRotationField, t => t.eulerAngles,
                (t, newVector) => t.eulerAngles = newVector);

            rotationField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean().DeltaAngle();

                CleanTransforms();
                ApplyToTargets(t => t.localEulerAngles = clearVector, "Set Local Rotation");

                rotationField.SetValueWithoutNotify(clearVector);
                globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles.DeltaAngle());
            });

            globalRotationField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean().DeltaAngle();

                CleanTransforms();
                ApplyToTargets(t => t.eulerAngles = clearVector, "Set World Rotation");

                rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.DeltaAngle());
                globalRotationField.SetValueWithoutNotify(clearVector);
            });

            rotationField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = "m_LocalRotation");
                evt.menu.AppendAction("Search for same Property", _ =>
                {
                    var rot = theTarget.localEulerAngles;
                    var context = SearchService.CreateContext("scene",
                        $"h:t:Transform #m_LocalRotation=({rot.x},{rot.y},{rot.z})");

                    SearchService.ShowWindow(context);
                });
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Copy Euler Angles",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(rotationField.value));
                evt.menu.AppendAction("Copy Quaternion",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatQuaternionLikeUnity(theTarget.localRotation));

                var canBePasteToVector3 = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                var canBePasteToQuaternion =
                    TryParseUnityQuaternion(EditorGUIUtility.systemCopyBuffer, out var pasteQuaternion);

                evt.menu.AppendAction("Paste", _ =>
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
                evt.menu.AppendAction("Copy Euler Angles",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(globalRotationField.value));
                evt.menu.AppendAction("Copy Quaternion",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatQuaternionLikeUnity(theTarget.rotation));

                var canBePasteToVector3 = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                var canBePasteToQuaternion =
                    TryParseUnityQuaternion(EditorGUIUtility.systemCopyBuffer, out var pasteQuaternion);

                evt.menu.AppendAction("Paste", _ =>
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

            var scaleField = tree.Q<Vector3Field>("scale");
            var lossyScaleField = tree.Q<Vector3Field>("lossyScale");

            lossyScaleField.value = theTarget.lossyScale;

            Vector3FieldApplyParsedInput(scaleField, t => t.localScale, (t, newVector) => t.localScale = newVector);
            Vector3FieldApplyParsedInput(lossyScaleField, t => t.lossyScale,
                (t, newVector) => t.SetLossyScale(newVector));

            scaleField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean();

                CleanTransforms();

                scaleField.SetValueWithoutNotify(clearVector);
                lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);
            });

            lossyScaleField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean();

                CleanTransforms();

                scaleField.SetValueWithoutNotify(theTarget.localScale);
                lossyScaleField.SetValueWithoutNotify(clearVector);
            });

            lossyScaleField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(lossyScaleField.value));
                var canBePaste = TryParseUnityVector3(EditorGUIUtility.systemCopyBuffer, out var pasteVector3);
                evt.menu.AppendAction("Paste", _ => lossyScaleField.value = pasteVector3,
                    canBePaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));

            var resetLocalPositionButton = tree.Q<Button>("resetLocalPosition");
            resetLocalPositionButton.clicked += ResetLocalPosition;

            var resetLocalRotationButton = tree.Q<Button>("resetLocalRotation");
            resetLocalRotationButton.clicked += ResetLocalRotation;

            var resetLocalScaleButton = tree.Q<Button>("resetLocalScale");
            resetLocalScaleButton.clicked += ResetLocalScale;

            var resetLocalAllButton = tree.Q<Button>("resetLocalAll");
            resetLocalAllButton.clicked += () =>
            {
                ResetLocalPosition();
                ResetLocalRotation();
                ResetLocalScale();
            };

            var resetGlobalPositionButton = tree.Q<Button>("resetGlobalPosition");
            resetGlobalPositionButton.clicked += ResetGlobalPosition;

            var resetGlobalRotationButton = tree.Q<Button>("resetGlobalRotation");
            resetGlobalRotationButton.clicked += ResetGlobalRotation;

            var resetGlobalScaleButton = tree.Q<Button>("resetGlobalScale");
            resetGlobalScaleButton.clicked += ResetGlobalScale;

            var resetGlobalAllButton = tree.Q<Button>("resetGlobalAll");
            resetGlobalAllButton.clicked += () =>
            {
                ResetGlobalPosition();
                ResetGlobalRotation();
                ResetGlobalScale();
            };

            var randomLocalPositionButton = tree.Q<Button>("randomLocalPosition");
            randomLocalPositionButton.clicked += RandomLocalPosition;

            var randomLocalRotationButton = tree.Q<Button>("randomLocalRotation");
            randomLocalRotationButton.clicked += RandomLocalRotation;

            var randomLocalScaleButton = tree.Q<Button>("randomLocalScale");
            randomLocalScaleButton.clicked += RandomLocalScale;

            var randomLocalAllButton = tree.Q<Button>("randomLocalAll");
            randomLocalAllButton.clicked += () =>
            {
                RandomLocalPosition();
                RandomLocalRotation();
                RandomLocalScale();
            };

            var randomGlobalPositionButton = tree.Q<Button>("randomGlobalPosition");
            randomGlobalPositionButton.clicked += RandomGlobalPosition;

            var randomGlobalRotationButton = tree.Q<Button>("randomGlobalRotation");
            randomGlobalRotationButton.clicked += RandomGlobalRotation;

            var randomGlobalScaleButton = tree.Q<Button>("randomGlobalScale");
            randomGlobalScaleButton.clicked += RandomGlobalScale;

            var randomGlobalAllButton = tree.Q<Button>("randomGlobalAll");
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
                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(data);
            };

            var pasteLocalTransformButton = tree.Q<Button>("pasteLocalTransform");
            pasteLocalTransformButton.clicked += () =>
            {
                if (!TryParsePRSData(EditorGUIUtility.systemCopyBuffer, out var data)) return;
                ApplyToTargets(t =>
                {
                    t.localPosition = data.position;
                    t.localEulerAngles = data.eulerAngles;
                    t.localScale = data.scale;
                }, "Paste Local Transform");
                positionField.value = data.position;
                rotationField.value = data.eulerAngles;
                scaleField.value = data.scale;
            };

            var copyGlobalTransformButton = tree.Q<Button>("copyGlobalTransform");
            copyGlobalTransformButton.clicked += () =>
            {
                var data = new PRSData(theTarget.position, theTarget.eulerAngles, theTarget.lossyScale);
                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(data);
            };

            var pasteGlobalTransformButton = tree.Q<Button>("pasteGlobalTransform");
            pasteGlobalTransformButton.clicked += () =>
            {
                if (!TryParsePRSData(EditorGUIUtility.systemCopyBuffer, out var data)) return;
                ApplyToTargets(t =>
                {
                    t.position = data.position;
                    t.eulerAngles = data.eulerAngles;
                    t.SetLossyScale(data.scale);
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

            var boundsSizeField = tree.Q<Vector3Field>("boundsSize");
            var boundsSizeFieldFloatFields = boundsSizeField.Query<FloatField>().ToList() ?? new List<FloatField>();

            foreach (var floatField in boundsSizeFieldFloatFields)
                floatField.isReadOnly = true;

            boundsSizeField.style.display = DisplayStyle.None;
            boundsSizeField.schedule.Execute(() =>
            {
                if (!theTarget.TryGetComponent(out Renderer r))
                {
                    boundsSizeField.style.display = DisplayStyle.None;
                    return;
                }

                boundsSizeField.value = r.bounds.size;
                boundsSizeField.style.display = DisplayStyle.Flex;
            }).Every(100);

            boundsSizeField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy",
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(boundsSizeField.value));
            }));

            var hierarchyPathField = tree.Q<TextField>("hierarchyPath");
            hierarchyPathField.schedule.Execute(() => hierarchyPathField.SetValueWithoutNotify(theTarget.FullName()))
                .Every(1000);

            hierarchyPathField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy", _ => EditorGUIUtility.systemCopyBuffer = hierarchyPathField.value);
            }));

            var fatherField = tree.Q<ObjectField>("father");
            fatherField.SetEnabled(false);

            var childrenList = tree.Q<ListView>("childrenList");
            var childrenSizeTextField = childrenList.Q<TextField>("unity-list-view__size-field");
            childrenSizeTextField.isReadOnly = true;

            var childrenListFoldout = childrenList.Q<Foldout>();
            childrenListFoldout.value = IsChildrenListExpanded;
            childrenListFoldout.RegisterValueChangedCallback(evt => IsChildrenListExpanded = evt.newValue);

            var children = theTarget.GetComponentsInChildren<Transform>(true)
                .Where(t => t != theTarget.transform)
                .ToList();

            childrenList.itemsSource = children;
            childrenList.makeItem = () =>
            {
                var field = new ObjectField
                {
                    objectType = typeof(Transform),
                    allowSceneObjects = true
                };
                field.SetEnabled(false);
                return field;
            };

            childrenList.bindItem = (element, index) =>
            {
                var field = (ObjectField)element;
                field.SetValueWithoutNotify(children[index]);
            };

            var alignToParentButton = tree.Q<Button>("alignToParent");
            alignToParentButton.clicked += () =>
            {
                ApplyToTargets(t =>
                {
                    if (t.parent == null) return;
                    t.position = t.parent.position;
                }, "Align to Parent");
            };

            var alignToGroundButton = tree.Q<Button>("alignToGround");
            alignToGroundButton.clicked += () =>
            {
                ApplyToTargets(t =>
                {
                    if (!Physics.Raycast(t.position + Vector3.up * 1000, Vector3.down, out var hit, 5000)) return;
                    t.position = hit.point;
                }, "Align to Ground");
            };

            return tree;

            void ResetLocalPosition()
            {
                ApplyToTargets(t => t.localPosition = Vector3.zero, "Reset Local Position");
                positionField.value = Vector3.zero;
            }

            void ResetLocalRotation()
            {
                ApplyToTargets(t => t.localEulerAngles = Vector3.zero, "Reset Local Rotation");
                rotationField.value = Vector3.zero;
            }

            void ResetLocalScale()
            {
                ApplyToTargets(t => t.localScale = Vector3.one, "Reset Local Scale");
                scaleField.value = Vector3.one;
            }

            void ResetGlobalPosition()
            {
                ApplyToTargets(t => t.position = Vector3.zero, "Reset World Position");
                globalPositionField.value = Vector3.zero;
            }

            void ResetGlobalRotation()
            {
                ApplyToTargets(t => t.eulerAngles = Vector3.zero, "Reset World Rotation");
                globalRotationField.value = Vector3.zero;
            }

            void ResetGlobalScale()
            {
                ApplyToTargets(t => t.SetLossyScale(Vector3.one), "Reset World Scale");
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
                var randomVector = (Random.insideUnitSphere * 360).Clean().DeltaAngle();
                ApplyToTargets(t => t.localEulerAngles = randomVector, "Random Local Rotation");
                rotationField.value = randomVector;
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
                var randomVector = (Random.insideUnitSphere * 360).Clean().DeltaAngle();
                ApplyToTargets(t => t.eulerAngles = randomVector, "Random World Rotation");
                globalRotationField.value = randomVector;
            }

            void RandomGlobalScale()
            {
                var randomVector = (Random.insideUnitSphere * 2).Clean();
                ApplyToTargets(t => t.SetLossyScale(t.lossyScale + randomVector), "Random World Scale");
                lossyScaleField.value += randomVector;
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

            try
            {
                data = JsonUtility.FromJson<PRSData>(json);

                if (data.scale == default && data.position == default && data.eulerAngles == default)
                    return json.Contains("position") || json.Contains("eulerAngles") || json.Contains("scale");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void CleanTransforms(float threshold = 0.0001f)
        {
            foreach (var obj in targets)
            {
                if (obj is not Transform t) continue;

                Undo.RecordObject(t, "Clean Transform");

                t.localPosition = t.localPosition.Clean(threshold);
                t.localEulerAngles = t.localEulerAngles.Clean(threshold);
                t.localScale = t.localScale.Clean(threshold);
            }
        }

        private void ApplyToTargets(Action<Transform> action, string undoMessage)
        {
            Undo.RecordObjects(targets, undoMessage);

            foreach (var obj in targets)
                if (obj is Transform t)
                    action(t);
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
                        p.x = v;
                        setter(t, p);
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
                        p.y = v;
                        setter(t, p);
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
                        p.z = v;
                        setter(t, p);
                    });
                if (parsed) onParsed?.Invoke();
            }).Every(1000);
        }
    }
}