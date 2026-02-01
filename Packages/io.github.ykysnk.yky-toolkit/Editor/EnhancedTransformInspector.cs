using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.ykyToolkit.Editor.UIElements;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ObjectField = UnityEditor.UIElements.ObjectField;
using Random = UnityEngine.Random;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public enum AlignMode
    {
        Pivot,
        Bottom,
        Center
    }

    // TODO: Simple icon buttons, Scale button
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
            defaultUIFoldOut.SetValueWithoutNotify(IsDefaultUIExpanded);
            defaultUIFoldOut.RegisterValueChangedCallback(evt => IsDefaultUIExpanded = evt.newValue);

            var defaultGUI = tree.Q<IMGUIContainer>("defaultGUI");
            defaultGUI.onGUIHandler = () => _defaultEditor?.OnInspectorGUI();

            var positionField = tree.Q<Vector3FieldExtra>("position");
            var globalPositionField = tree.Q<Vector3FieldExtra>("globalPosition");

            globalPositionField.SetValueWithoutNotify(theTarget.position);

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
                var clearVector = evt.newValue.Clean().DeltaAngle();

                CleanTransforms();

                if (rotationEditing)
                    ApplyToTargets(t => t.localEulerAngles = clearVector, "Set Local Rotation");

                rotationField.SetValueWithoutNotify(clearVector);
                globalRotationField.SetValueWithoutNotify(theTarget.eulerAngles.DeltaAngle());
            });

            var globalRotationEditing = false;

            globalRotationField.RegisterCallback<FocusInEvent>(_ => globalRotationEditing = true);
            globalRotationField.RegisterCallback<FocusOutEvent>(_ => globalRotationEditing = false);

            globalRotationField.RegisterValueChangedCallback(evt =>
            {
                var clearVector = evt.newValue.Clean().DeltaAngle();

                CleanTransforms();

                if (globalRotationEditing)
                    ApplyToTargets(t => t.eulerAngles = clearVector, "Set World Rotation");

                rotationField.SetValueWithoutNotify(theTarget.localEulerAngles.DeltaAngle());
                globalRotationField.SetValueWithoutNotify(clearVector);
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

            var scaleField = tree.Q<Vector3FieldExtra>("scale");
            var lossyScaleField = tree.Q<Vector3FieldExtra>("lossyScale");

            lossyScaleField.SetValueWithoutNotify(theTarget.lossyScale);

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

            var resetLocalAllButton = tree.Q<IconButton>("resetLocalAll");
            resetLocalAllButton.style.backgroundImage = new(EditorGUIUtils.IconTexture("refresh") as Texture2D);
            resetLocalAllButton.clicked += () =>
            {
                ResetLocalPosition();
                ResetLocalRotation();
                ResetLocalScale();
                // dumb rotation field issue
                // resetLocalAllButton.schedule.Execute(ResetLocalRotation);
            };

            var resetGlobalAllButton = tree.Q<IconButton>("resetGlobalAll");
            resetGlobalAllButton.style.backgroundImage = new(EditorGUIUtils.IconTexture("refresh") as Texture2D);
            resetGlobalAllButton.clicked += () =>
            {
                ResetGlobalPosition();
                ResetGlobalRotation();
                ResetGlobalScale();
            };

            var randomLocalAllButton = tree.Q<IconButton>("randomLocalAll");
            randomLocalAllButton.style.backgroundImage =
                new(EditorGUIUtils.IconTexture("preaudioloopoff") as Texture2D);
            randomLocalAllButton.clicked += () =>
            {
                RandomLocalPosition();
                RandomLocalRotation();
                RandomLocalScale();
            };

            var randomGlobalAllButton = tree.Q<IconButton>("randomGlobalAll");
            randomGlobalAllButton.style.backgroundImage =
                new(EditorGUIUtils.IconTexture("preaudioloopoff") as Texture2D);
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
                evt.menu.AppendAction("label.copy".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = FormatVector3LikeUnity(boundsSizeField.value));
            }));

            var hierarchyPathField = tree.Q<TextField>("hierarchyPath");
            hierarchyPathField.schedule.Execute(() => hierarchyPathField.SetValueWithoutNotify(theTarget.FullName()))
                .Every(1000);

            hierarchyPathField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("label.copy".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = hierarchyPathField.value);
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
            clearParentButton.clicked += () => ApplyToTargets(t => t.SetParent(null, true), "Clear Parent");

            var helpFoldout = tree.Q<Foldout>("helpFoldout");
            helpFoldout.value = IsHelpExpanded;
            helpFoldout.RegisterValueChangedCallback(evt => IsHelpExpanded = evt.newValue);

            var helpText = tree.Q<Label>("helpText");
            helpText.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("label.copy".S(), _ => EditorGUIUtility.systemCopyBuffer = helpText.text)));

            return tree;

            void CleanField()
            {
                positionField.value = positionField.value.Clean();
                rotationField.value = rotationField.value.Clean().DeltaAngle();
                scaleField.value = scaleField.value.Clean();
            }

            void ResetLocalPosition()
            {
                ApplyToTargets(t => t.localPosition = Vector3.zero, "Reset Local Position");
                positionField.value = Vector3.zero;
            }

            void ResetLocalRotation()
            {
                rotationEditing = true;
                ApplyToTargets(t => t.localEulerAngles = Vector3.zero, "Reset Local Rotation");
                rotationField.value = Vector3.zero;
                rotationField.schedule.Execute(() => rotationEditing = false);
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
                rotationEditing = true;
                ApplyToTargets(t => t.eulerAngles = Vector3.zero, "Reset World Rotation");
                globalRotationField.value = Vector3.zero;
                globalPositionField.schedule.Execute(() => rotationEditing = false);
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
                t.localEulerAngles = t.localEulerAngles.Clean(threshold).DeltaAngle();
                t.localScale = t.localScale.Clean(threshold);
            }
        }

        private void CleanWorldTransforms(float threshold = 0.0001f)
        {
            foreach (var obj in targets)
            {
                if (obj is not Transform t) continue;

                Undo.RecordObject(t, "Clean World Transform");

                t.position = t.position.Clean(threshold);
                t.eulerAngles = t.eulerAngles.Clean(threshold).DeltaAngle();
                t.SetLossyScale(t.lossyScale.Clean(threshold));
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

            t.position = new(
                t.position.x,
                groundPoint.y - offset,
                t.position.z
            );
        }
    }
}