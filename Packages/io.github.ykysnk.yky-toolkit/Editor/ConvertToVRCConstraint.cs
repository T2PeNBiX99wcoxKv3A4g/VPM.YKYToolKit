#if YKYTOOLKIT_VRCBASE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class ConvertToVRCConstraint
    {
        [MenuItem("GameObject/YKYToolkit/Convert Unity Constraint To VRC Constraint", false, Util.Five)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Convert Unity Constraint To VRC Constraint", false, Util.Three2)]
        private static void ConvertToVRC(MenuCommand menuCommand)
        {
            if (!Util.ShouldExecute(menuCommand)) return;
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;
            ConvertToVRCAsync(selectedObjects).Forget();
        }

        private static async UniTask ConvertToVRCAsync(GameObject[] selectedObjects)
        {
            var stopwatch = Stopwatch.StartNew();
            var searchResult = new Dictionary<GameObject, Dictionary<Transform, ConstraintData[]>>();
            var doneResult = new HashSet<Transform>();
            var doneFound = new HashSet<Transform>();

            foreach (var selected in selectedObjects)
            {
                var result = new Dictionary<Transform, ConstraintData[]>();
                var stack = new Stack<Transform>();

                stack.Push(selected.transform);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (!doneResult.Contains(current))
                    {
                        var constraints = current.GetComponents<Component>().Where(x => x && x is IConstraint)
                            .Cast<IConstraint>().ToArray();

                        if (constraints.Length > 0)
                            result.TryAdd(current, constraints.Select(x => new ConstraintData(x)).ToArray());

                        foreach (Transform child in current)
                            stack.Push(child);

                        doneResult.Add(current);
                    }

                    if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }

                searchResult.TryAdd(selected, result);

                if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                await UniTask.Yield();
                stopwatch.Restart();
            }

            foreach (var (selected, result) in searchResult)
            {
                if (!selected || result == null) continue;

                foreach (var (found, datas) in result)
                {
                    if (!found || !doneFound.Add(found)) continue;

                    Undo.RecordObject(found.gameObject, "Convert to VRC Constraint");

                    // Collected up front: destroying components while walking them shifts the indices and would skip
                    // every constraint that follows a destroyed one.
                    var unityConstraints =
                        found.GetComponents<Component>().Where(x => x && x is IConstraint).ToArray();

                    foreach (var data in datas)
                        switch (data.Type)
                        {
                            case ConstraintType.Position:
                            {
                                var vrcPositionConstraint = Undo.AddComponent<VRCPositionConstraint>(found.gameObject);
                                vrcPositionConstraint.IsActive = data.IsActive;
                                vrcPositionConstraint.Locked = data.Locked;
                                vrcPositionConstraint.GlobalWeight = data.GlobalWeight;
                                vrcPositionConstraint.enabled = data.Enabled;

                                if (data.ExtraData is PositionExtraData extraData)
                                {
                                    vrcPositionConstraint.PositionAtRest = extraData.PositionAtRest;
                                    vrcPositionConstraint.PositionOffset = extraData.PositionOffset;
                                    vrcPositionConstraint.AffectsPositionX = extraData.AffectsPositionX;
                                    vrcPositionConstraint.AffectsPositionY = extraData.AffectsPositionY;
                                    vrcPositionConstraint.AffectsPositionZ = extraData.AffectsPositionZ;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new VRCConstraintSource(sourceData.SourceTransform, sourceData.Weight);
                                    vrcPositionConstraint.Sources.Add(source);
                                }

                                break;
                            }
                            case ConstraintType.Rotation:
                            {
                                var vrcRotationConstraint = Undo.AddComponent<VRCRotationConstraint>(found.gameObject);
                                vrcRotationConstraint.IsActive = data.IsActive;
                                vrcRotationConstraint.Locked = data.Locked;
                                vrcRotationConstraint.GlobalWeight = data.GlobalWeight;
                                vrcRotationConstraint.enabled = data.Enabled;

                                if (data.ExtraData is RotationExtraData extraData)
                                {
                                    vrcRotationConstraint.RotationAtRest = extraData.RotationAtRest;
                                    vrcRotationConstraint.RotationOffset = extraData.RotationOffset;
                                    vrcRotationConstraint.AffectsRotationX = extraData.AffectsRotationX;
                                    vrcRotationConstraint.AffectsRotationY = extraData.AffectsRotationY;
                                    vrcRotationConstraint.AffectsRotationZ = extraData.AffectsRotationZ;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new VRCConstraintSource(sourceData.SourceTransform, sourceData.Weight);
                                    vrcRotationConstraint.Sources.Add(source);
                                }

                                break;
                            }
                            case ConstraintType.Scale:
                            {
                                var vrcScaleConstraint = Undo.AddComponent<VRCScaleConstraint>(found.gameObject);
                                vrcScaleConstraint.IsActive = data.IsActive;
                                vrcScaleConstraint.Locked = data.Locked;
                                vrcScaleConstraint.GlobalWeight = data.GlobalWeight;
                                vrcScaleConstraint.enabled = data.Enabled;

                                if (data.ExtraData is ScaleExtraData extraData)
                                {
                                    vrcScaleConstraint.ScaleAtRest = extraData.ScaleAtRest;
                                    vrcScaleConstraint.ScaleOffset = extraData.ScaleOffset;
                                    vrcScaleConstraint.AffectsScaleX = extraData.AffectsScaleX;
                                    vrcScaleConstraint.AffectsScaleY = extraData.AffectsScaleY;
                                    vrcScaleConstraint.AffectsScaleZ = extraData.AffectsScaleZ;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new VRCConstraintSource(sourceData.SourceTransform, sourceData.Weight);
                                    vrcScaleConstraint.Sources.Add(source);
                                }

                                break;
                            }
                            case ConstraintType.Parent:
                            {
                                var vrcParentConstraint = Undo.AddComponent<VRCParentConstraint>(found.gameObject);
                                vrcParentConstraint.IsActive = data.IsActive;
                                vrcParentConstraint.Locked = data.Locked;
                                vrcParentConstraint.GlobalWeight = data.GlobalWeight;
                                vrcParentConstraint.enabled = data.Enabled;

                                if (data.ExtraData is ParentExtraData extraData)
                                {
                                    vrcParentConstraint.PositionAtRest = extraData.PositionAtRest;
                                    vrcParentConstraint.RotationAtRest = extraData.RotationAtRest;
                                    vrcParentConstraint.AffectsPositionX = extraData.AffectsPositionX;
                                    vrcParentConstraint.AffectsPositionY = extraData.AffectsPositionY;
                                    vrcParentConstraint.AffectsPositionZ = extraData.AffectsPositionZ;
                                    vrcParentConstraint.AffectsRotationX = extraData.AffectsRotationX;
                                    vrcParentConstraint.AffectsRotationY = extraData.AffectsRotationY;
                                    vrcParentConstraint.AffectsRotationZ = extraData.AffectsRotationZ;

                                    for (var i = 0; i < data.SourceDatas.Length; i++)
                                    {
                                        var sourceData = data.SourceDatas[i];

                                        // Unity keeps these arrays the same length as the sources, but serialized data
                                        // from another version (or the VRC constructor's empty arrays) may not.
                                        var positionOffset = i < extraData.PositionOffsets.Length
                                            ? extraData.PositionOffsets[i]
                                            : Vector3.zero;
                                        var rotationOffset = i < extraData.RotationOffsets.Length
                                            ? extraData.RotationOffsets[i]
                                            : Vector3.zero;

                                        var source = new VRCConstraintSource(sourceData.SourceTransform,
                                            sourceData.Weight, positionOffset, rotationOffset);
                                        vrcParentConstraint.Sources.Add(source);
                                    }
                                }

                                break;
                            }
                            case ConstraintType.LookAt:
                            {
                                var vrcLookAtConstraint = Undo.AddComponent<VRCLookAtConstraint>(found.gameObject);
                                vrcLookAtConstraint.IsActive = data.IsActive;
                                vrcLookAtConstraint.Locked = data.Locked;
                                vrcLookAtConstraint.GlobalWeight = data.GlobalWeight;
                                vrcLookAtConstraint.enabled = data.Enabled;

                                if (data.ExtraData is LookAtExtraData extraData)
                                {
                                    vrcLookAtConstraint.RotationAtRest = extraData.RotationAtRest;
                                    vrcLookAtConstraint.RotationOffset = extraData.RotationOffset;
                                    vrcLookAtConstraint.Roll = extraData.Roll;
                                    vrcLookAtConstraint.UseUpTransform = extraData.UseUpTransform;
                                    vrcLookAtConstraint.WorldUpTransform = extraData.WorldUpTransform;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new VRCConstraintSource(sourceData.SourceTransform, sourceData.Weight);
                                    vrcLookAtConstraint.Sources.Add(source);
                                }

                                break;
                            }
                            case ConstraintType.Aim:
                            {
                                var vrcAimConstraint = Undo.AddComponent<VRCAimConstraint>(found.gameObject);
                                vrcAimConstraint.IsActive = data.IsActive;
                                vrcAimConstraint.Locked = data.Locked;
                                vrcAimConstraint.GlobalWeight = data.GlobalWeight;
                                vrcAimConstraint.enabled = data.Enabled;

                                if (data.ExtraData is AimExtraData extraData)
                                {
                                    vrcAimConstraint.RotationAtRest = extraData.RotationAtRest;
                                    vrcAimConstraint.RotationOffset = extraData.RotationOffset;
                                    vrcAimConstraint.AffectsRotationX = extraData.AffectsRotationX;
                                    vrcAimConstraint.AffectsRotationY = extraData.AffectsRotationY;
                                    vrcAimConstraint.AffectsRotationZ = extraData.AffectsRotationZ;
                                    vrcAimConstraint.AimAxis = extraData.AimAxis;
                                    vrcAimConstraint.UpAxis = extraData.UpAxis;
                                    vrcAimConstraint.WorldUp = extraData.WorldUp;
                                    vrcAimConstraint.WorldUpVector = extraData.WorldUpVector;
                                    vrcAimConstraint.WorldUpTransform = extraData.WorldUpTransform;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new VRCConstraintSource(sourceData.SourceTransform, sourceData.Weight);
                                    vrcAimConstraint.Sources.Add(source);
                                }

                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException(nameof(data.Type), data.Type,
                                    "Constraint type isn't supported");
                        }

                    // Destroyed only after every replacement exists, so a failure above cannot leave the object with
                    // its Unity constraints removed and nothing in their place.
                    foreach (var unityConstraint in unityConstraints)
                        Undo.DestroyObjectImmediate(unityConstraint);

                    EditorUtility.SetDirty(found.gameObject);

                    if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }

                if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                await UniTask.Yield();
                stopwatch.Restart();
            }
        }

        [MenuItem("GameObject/YKYToolkit/Convert VRC Constraint To Unity Constraint", false, Util.Five)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Convert VRC Constraint To Unity Constraint", false, Util.Three2)]
        private static void ConvertToUnity(MenuCommand menuCommand)
        {
            if (!Util.ShouldExecute(menuCommand)) return;
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;
            ConvertToUnity(selectedObjects).Forget();
        }

        private static async UniTask ConvertToUnity(GameObject[] selectedObjects)
        {
            var stopwatch = Stopwatch.StartNew();
            var searchResult = new Dictionary<GameObject, Dictionary<Transform, ConstraintData[]>>();
            var doneResult = new HashSet<Transform>();
            var doneFound = new HashSet<Transform>();

            foreach (var selected in selectedObjects)
            {
                var result = new Dictionary<Transform, ConstraintData[]>();
                var stack = new Stack<Transform>();

                stack.Push(selected.transform);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (!doneResult.Contains(current))
                    {
                        var constraints = current.GetComponents<VRCConstraintBase>().Where(x => x).ToArray();

                        if (constraints.Length > 0)
                            result.TryAdd(current, constraints.Select(x => new ConstraintData(x)).ToArray());

                        foreach (Transform child in current)
                            stack.Push(child);

                        doneResult.Add(current);
                    }

                    if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }

                searchResult.TryAdd(selected, result);

                if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                await UniTask.Yield();
                stopwatch.Restart();
            }

            foreach (var (selected, result) in searchResult)
            {
                if (!selected || result == null) continue;

                foreach (var (found, datas) in result)
                {
                    if (!found || !doneFound.Add(found)) continue;

                    Undo.RecordObject(found.gameObject, "Convert to Unity Constraint");

                    // Collected up front: destroying components while walking them shifts the indices and would skip
                    // every constraint that follows a destroyed one.
                    var vrcConstraints = found.GetComponents<VRCConstraintBase>().Where(x => x).ToArray();

                    foreach (var data in datas)
                        switch (data.Type)
                        {
                            case ConstraintType.Position:
                            {
                                var positionConstraint = Undo.AddComponent<PositionConstraint>(found.gameObject);
                                positionConstraint.constraintActive = data.IsActive;
                                positionConstraint.locked = data.Locked;
                                positionConstraint.weight = data.GlobalWeight;
                                positionConstraint.enabled = data.Enabled;

                                if (data.ExtraData is PositionExtraData extraData)
                                {
                                    positionConstraint.translationAtRest = extraData.PositionAtRest;
                                    positionConstraint.translationOffset = extraData.PositionOffset;

                                    var axis = Axis.None;

                                    if (extraData.AffectsPositionX) axis |= Axis.X;
                                    if (extraData.AffectsPositionY) axis |= Axis.Y;
                                    if (extraData.AffectsPositionZ) axis |= Axis.Z;

                                    positionConstraint.translationAxis = axis;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new ConstraintSource
                                    {
                                        sourceTransform = sourceData.SourceTransform,
                                        weight = sourceData.Weight
                                    };
                                    positionConstraint.AddSource(source);
                                }

                                break;
                            }
                            case ConstraintType.Rotation:
                            {
                                var rotationConstraint = Undo.AddComponent<RotationConstraint>(found.gameObject);
                                rotationConstraint.constraintActive = data.IsActive;
                                rotationConstraint.locked = data.Locked;
                                rotationConstraint.weight = data.GlobalWeight;
                                rotationConstraint.enabled = data.Enabled;

                                if (data.ExtraData is RotationExtraData extraData)
                                {
                                    rotationConstraint.rotationAtRest = extraData.RotationAtRest;
                                    rotationConstraint.rotationOffset = extraData.RotationOffset;

                                    var axis = Axis.None;

                                    if (extraData.AffectsRotationX) axis |= Axis.X;
                                    if (extraData.AffectsRotationY) axis |= Axis.Y;
                                    if (extraData.AffectsRotationZ) axis |= Axis.Z;

                                    rotationConstraint.rotationAxis = axis;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new ConstraintSource
                                    {
                                        sourceTransform = sourceData.SourceTransform,
                                        weight = sourceData.Weight
                                    };
                                    rotationConstraint.AddSource(source);
                                }

                                break;
                            }
                            case ConstraintType.Scale:
                            {
                                var scaleConstraint = Undo.AddComponent<ScaleConstraint>(found.gameObject);
                                scaleConstraint.constraintActive = data.IsActive;
                                scaleConstraint.locked = data.Locked;
                                scaleConstraint.weight = data.GlobalWeight;
                                scaleConstraint.enabled = data.Enabled;

                                if (data.ExtraData is ScaleExtraData extraData)
                                {
                                    scaleConstraint.scaleAtRest = extraData.ScaleAtRest;
                                    scaleConstraint.scaleOffset = extraData.ScaleOffset;

                                    var axis = Axis.None;

                                    if (extraData.AffectsScaleX) axis |= Axis.X;
                                    if (extraData.AffectsScaleY) axis |= Axis.Y;
                                    if (extraData.AffectsScaleZ) axis |= Axis.Z;

                                    scaleConstraint.scalingAxis = axis;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new ConstraintSource
                                    {
                                        sourceTransform = sourceData.SourceTransform,
                                        weight = sourceData.Weight
                                    };
                                    scaleConstraint.AddSource(source);
                                }

                                break;
                            }
                            case ConstraintType.Parent:
                            {
                                var parentConstraint = Undo.AddComponent<ParentConstraint>(found.gameObject);
                                parentConstraint.constraintActive = data.IsActive;
                                parentConstraint.locked = data.Locked;
                                parentConstraint.weight = data.GlobalWeight;
                                parentConstraint.enabled = data.Enabled;

                                if (data.ExtraData is ParentExtraData extraData)
                                {
                                    parentConstraint.translationAtRest = extraData.PositionAtRest;
                                    parentConstraint.rotationAtRest = extraData.RotationAtRest;

                                    var positionAxis = Axis.None;

                                    if (extraData.AffectsPositionX) positionAxis |= Axis.X;
                                    if (extraData.AffectsPositionY) positionAxis |= Axis.Y;
                                    if (extraData.AffectsPositionZ) positionAxis |= Axis.Z;

                                    parentConstraint.translationAxis = positionAxis;

                                    var rotationAxis = Axis.None;

                                    if (extraData.AffectsRotationX) rotationAxis |= Axis.X;
                                    if (extraData.AffectsRotationY) rotationAxis |= Axis.Y;
                                    if (extraData.AffectsRotationZ) rotationAxis |= Axis.Z;

                                    parentConstraint.rotationAxis = rotationAxis;

                                    var positionOffsets = new List<Vector3>();
                                    var rotationOffsets = new List<Vector3>();

                                    foreach (var sourceData in data.SourceDatas)
                                    {
                                        positionOffsets.Add(sourceData.ParentPositionOffset);
                                        rotationOffsets.Add(sourceData.ParentRotationOffset);

                                        var source = new ConstraintSource
                                        {
                                            sourceTransform = sourceData.SourceTransform,
                                            weight = sourceData.Weight
                                        };
                                        parentConstraint.AddSource(source);
                                    }

                                    parentConstraint.translationOffsets = positionOffsets.ToArray();
                                    parentConstraint.rotationOffsets = rotationOffsets.ToArray();
                                }

                                break;
                            }
                            case ConstraintType.LookAt:
                            {
                                var lookAtConstraint = Undo.AddComponent<LookAtConstraint>(found.gameObject);
                                lookAtConstraint.constraintActive = data.IsActive;
                                lookAtConstraint.locked = data.Locked;
                                lookAtConstraint.weight = data.GlobalWeight;
                                lookAtConstraint.enabled = data.Enabled;

                                if (data.ExtraData is LookAtExtraData extraData)
                                {
                                    lookAtConstraint.rotationAtRest = extraData.RotationAtRest;
                                    lookAtConstraint.rotationOffset = extraData.RotationOffset;
                                    lookAtConstraint.roll = extraData.Roll;
                                    lookAtConstraint.useUpObject = extraData.UseUpTransform;
                                    lookAtConstraint.worldUpObject = extraData.WorldUpTransform;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new ConstraintSource
                                    {
                                        sourceTransform = sourceData.SourceTransform,
                                        weight = sourceData.Weight
                                    };
                                    lookAtConstraint.AddSource(source);
                                }

                                break;
                            }
                            case ConstraintType.Aim:
                            {
                                var aimConstraint = Undo.AddComponent<AimConstraint>(found.gameObject);
                                aimConstraint.constraintActive = data.IsActive;
                                aimConstraint.locked = data.Locked;
                                aimConstraint.weight = data.GlobalWeight;
                                aimConstraint.enabled = data.Enabled;

                                if (data.ExtraData is AimExtraData extraData)
                                {
                                    aimConstraint.rotationAtRest = extraData.RotationAtRest;
                                    aimConstraint.rotationOffset = extraData.RotationOffset;

                                    var axis = Axis.None;

                                    if (extraData.AffectsRotationX) axis |= Axis.X;
                                    if (extraData.AffectsRotationY) axis |= Axis.Y;
                                    if (extraData.AffectsRotationZ) axis |= Axis.Z;

                                    aimConstraint.rotationAxis = axis;

                                    aimConstraint.aimVector = extraData.AimAxis;
                                    aimConstraint.upVector = extraData.UpAxis;
                                    aimConstraint.worldUpType = (AimConstraint.WorldUpType)(int)extraData.WorldUp;
                                    aimConstraint.worldUpVector = extraData.WorldUpVector;
                                    aimConstraint.worldUpObject = extraData.WorldUpTransform;
                                }

                                foreach (var sourceData in data.SourceDatas)
                                {
                                    var source = new ConstraintSource
                                    {
                                        sourceTransform = sourceData.SourceTransform,
                                        weight = sourceData.Weight
                                    };
                                    aimConstraint.AddSource(source);
                                }

                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException(nameof(data.Type), data.Type,
                                    "Constraint type isn't supported");
                        }

                    // Destroyed only after every replacement exists, so a failure above cannot leave the object with
                    // its VRC constraints removed and nothing in their place.
                    foreach (var vrcConstraint in vrcConstraints)
                        Undo.DestroyObjectImmediate(vrcConstraint);

                    EditorUtility.SetDirty(found.gameObject);

                    if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }

                if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                await UniTask.Yield();
                stopwatch.Restart();
            }
        }

        private class ConstraintData
        {
            public readonly bool Enabled;
            public readonly IConstraintExtraData ExtraData;
            public readonly float GlobalWeight;
            public readonly bool IsActive;
            public readonly bool Locked;
            public readonly ConstraintSourceData[] SourceDatas;
            public readonly ConstraintType Type;

            public ConstraintData(IConstraint constraint)
            {
                Type = constraint switch
                {
                    PositionConstraint => ConstraintType.Position,
                    RotationConstraint => ConstraintType.Rotation,
                    ScaleConstraint => ConstraintType.Scale,
                    ParentConstraint => ConstraintType.Parent,
                    LookAtConstraint => ConstraintType.LookAt,
                    AimConstraint => ConstraintType.Aim,
                    _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint,
                        "Constraint type isn't supported")
                };
                GlobalWeight = constraint.weight;
                IsActive = constraint.constraintActive;
                Locked = constraint.locked;
                Enabled = (constraint as Behaviour)?.enabled ?? true;

                var sourceList = new List<ConstraintSource>();
                constraint.GetSources(sourceList);
                SourceDatas = sourceList.Select(source => new ConstraintSourceData(source)).ToArray();

                ExtraData = Type switch
                {
                    ConstraintType.Position => new PositionExtraData(constraint),
                    ConstraintType.Rotation => new RotationExtraData(constraint),
                    ConstraintType.Scale => new ScaleExtraData(constraint),
                    ConstraintType.Parent => new ParentExtraData(constraint),
                    ConstraintType.LookAt => new LookAtExtraData(constraint),
                    ConstraintType.Aim => new AimExtraData(constraint),
                    _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Constraint type isn't supported")
                };
            }

            public ConstraintData(VRCConstraintBase constraint)
            {
                Type = constraint switch
                {
                    VRCPositionConstraint => ConstraintType.Position,
                    VRCRotationConstraint => ConstraintType.Rotation,
                    VRCScaleConstraint => ConstraintType.Scale,
                    VRCParentConstraint => ConstraintType.Parent,
                    VRCLookAtConstraint => ConstraintType.LookAt,
                    VRCAimConstraint => ConstraintType.Aim,
                    _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint,
                        "Constraint type isn't supported")
                };
                GlobalWeight = constraint.GlobalWeight;
                IsActive = constraint.IsActive;
                Locked = constraint.Locked;
                Enabled = constraint.enabled;

                SourceDatas = constraint.Sources.Select(source => new ConstraintSourceData(source)).ToArray();

                ExtraData = Type switch
                {
                    ConstraintType.Position => new PositionExtraData(constraint),
                    ConstraintType.Rotation => new RotationExtraData(constraint),
                    ConstraintType.Scale => new ScaleExtraData(constraint),
                    ConstraintType.Parent => new ParentExtraData(constraint),
                    ConstraintType.LookAt => new LookAtExtraData(constraint),
                    ConstraintType.Aim => new AimExtraData(constraint),
                    _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Constraint type isn't supported")
                };
            }
        }

        private class ConstraintSourceData
        {
            public readonly Vector3 ParentPositionOffset;
            public readonly Vector3 ParentRotationOffset;
            public readonly Transform SourceTransform;
            public readonly float Weight;

            public ConstraintSourceData(ConstraintSource source)
            {
                SourceTransform = source.sourceTransform;
                Weight = source.weight;
            }

            public ConstraintSourceData(VRCConstraintSource source)
            {
                SourceTransform = source.SourceTransform;
                Weight = source.Weight;
                ParentPositionOffset = source.ParentPositionOffset;
                ParentRotationOffset = source.ParentRotationOffset;
            }
        }

        private interface IConstraintExtraData
        {
        }

        private class PositionExtraData : IConstraintExtraData
        {
            public readonly bool AffectsPositionX;
            public readonly bool AffectsPositionY;
            public readonly bool AffectsPositionZ;
            public readonly Vector3 PositionAtRest;
            public readonly Vector3 PositionOffset;

            public PositionExtraData(IConstraint constraint)
            {
                if (constraint is not PositionConstraint positionConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(PositionConstraint)}");

                PositionOffset = positionConstraint.translationOffset;
                PositionAtRest = positionConstraint.translationAtRest;
                AffectsPositionX = (positionConstraint.translationAxis & Axis.X) != 0;
                AffectsPositionY = (positionConstraint.translationAxis & Axis.Y) != 0;
                AffectsPositionZ = (positionConstraint.translationAxis & Axis.Z) != 0;
            }

            public PositionExtraData(VRCConstraintBase constraint)
            {
                if (constraint is not VRCPositionConstraint positionConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(VRCPositionConstraint)}");

                PositionOffset = positionConstraint.PositionOffset;
                PositionAtRest = positionConstraint.PositionAtRest;
                AffectsPositionX = positionConstraint.AffectsPositionX;
                AffectsPositionY = positionConstraint.AffectsPositionY;
                AffectsPositionZ = positionConstraint.AffectsPositionZ;
            }
        }

        private class RotationExtraData : IConstraintExtraData
        {
            public readonly bool AffectsRotationX;
            public readonly bool AffectsRotationY;
            public readonly bool AffectsRotationZ;
            public readonly Vector3 RotationAtRest;
            public readonly Vector3 RotationOffset;

            public RotationExtraData(IConstraint constraint)
            {
                if (constraint is not RotationConstraint rotationConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(RotationConstraint)}");

                RotationOffset = rotationConstraint.rotationOffset;
                RotationAtRest = rotationConstraint.rotationAtRest;
                AffectsRotationX = (rotationConstraint.rotationAxis & Axis.X) != 0;
                AffectsRotationY = (rotationConstraint.rotationAxis & Axis.Y) != 0;
                AffectsRotationZ = (rotationConstraint.rotationAxis & Axis.Z) != 0;
            }

            public RotationExtraData(VRCConstraintBase constraint)
            {
                if (constraint is not VRCRotationConstraint rotationConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(VRCRotationConstraint)}");

                RotationOffset = rotationConstraint.RotationOffset;
                RotationAtRest = rotationConstraint.RotationAtRest;
                AffectsRotationX = rotationConstraint.AffectsRotationX;
                AffectsRotationY = rotationConstraint.AffectsRotationY;
                AffectsRotationZ = rotationConstraint.AffectsRotationZ;
            }
        }

        private class ScaleExtraData : IConstraintExtraData
        {
            public readonly bool AffectsScaleX;
            public readonly bool AffectsScaleY;
            public readonly bool AffectsScaleZ;
            public readonly Vector3 ScaleAtRest;
            public readonly Vector3 ScaleOffset;

            public ScaleExtraData(IConstraint constraint)
            {
                if (constraint is not ScaleConstraint scaleConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(ScaleConstraint)}");

                ScaleOffset = scaleConstraint.scaleOffset;
                ScaleAtRest = scaleConstraint.scaleAtRest;
                AffectsScaleX = (scaleConstraint.scalingAxis & Axis.X) != 0;
                AffectsScaleY = (scaleConstraint.scalingAxis & Axis.Y) != 0;
                AffectsScaleZ = (scaleConstraint.scalingAxis & Axis.Z) != 0;
            }

            public ScaleExtraData(VRCConstraintBase constraint)
            {
                if (constraint is not VRCScaleConstraint scaleConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(VRCScaleConstraint)}");

                ScaleOffset = scaleConstraint.ScaleOffset;
                ScaleAtRest = scaleConstraint.ScaleAtRest;
                AffectsScaleX = scaleConstraint.AffectsScaleX;
                AffectsScaleY = scaleConstraint.AffectsScaleY;
                AffectsScaleZ = scaleConstraint.AffectsScaleZ;
            }
        }

        private class ParentExtraData : IConstraintExtraData
        {
            public readonly bool AffectsPositionX;
            public readonly bool AffectsPositionY;
            public readonly bool AffectsPositionZ;
            public readonly bool AffectsRotationX;
            public readonly bool AffectsRotationY;
            public readonly bool AffectsRotationZ;
            public readonly Vector3 PositionAtRest;
            public readonly Vector3[] PositionOffsets;
            public readonly Vector3 RotationAtRest;
            public readonly Vector3[] RotationOffsets;

            public ParentExtraData(IConstraint constraint)
            {
                if (constraint is not ParentConstraint parentConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(ParentConstraint)}");

                PositionAtRest = parentConstraint.translationAtRest;
                PositionOffsets = parentConstraint.translationOffsets.ToArray();
                AffectsPositionX = (parentConstraint.translationAxis & Axis.X) != 0;
                AffectsPositionY = (parentConstraint.translationAxis & Axis.Y) != 0;
                AffectsPositionZ = (parentConstraint.translationAxis & Axis.Z) != 0;

                RotationAtRest = parentConstraint.rotationAtRest;
                RotationOffsets = parentConstraint.rotationOffsets.ToArray();
                AffectsRotationX = (parentConstraint.rotationAxis & Axis.X) != 0;
                AffectsRotationY = (parentConstraint.rotationAxis & Axis.Y) != 0;
                AffectsRotationZ = (parentConstraint.rotationAxis & Axis.Z) != 0;
            }

            public ParentExtraData(VRCConstraintBase constraint)
            {
                if (constraint is not VRCParentConstraint parentConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(VRCParentConstraint)}");

                PositionAtRest = parentConstraint.PositionAtRest;
                PositionOffsets = Array.Empty<Vector3>();
                AffectsPositionX = parentConstraint.AffectsPositionX;
                AffectsPositionY = parentConstraint.AffectsPositionY;
                AffectsPositionZ = parentConstraint.AffectsPositionZ;

                RotationAtRest = parentConstraint.RotationAtRest;
                RotationOffsets = Array.Empty<Vector3>();
                AffectsRotationX = parentConstraint.AffectsRotationX;
                AffectsRotationY = parentConstraint.AffectsRotationY;
                AffectsRotationZ = parentConstraint.AffectsRotationZ;
            }
        }

        private class LookAtExtraData : IConstraintExtraData
        {
            public readonly float Roll;
            public readonly Vector3 RotationAtRest;
            public readonly Vector3 RotationOffset;
            public readonly bool UseUpTransform;
            public readonly Transform WorldUpTransform;

            public LookAtExtraData(IConstraint constraint)
            {
                if (constraint is not LookAtConstraint lookAtConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(LookAtConstraint)}");

                RotationAtRest = lookAtConstraint.rotationAtRest;
                RotationOffset = lookAtConstraint.rotationOffset;
                Roll = lookAtConstraint.roll;
                UseUpTransform = lookAtConstraint.useUpObject;
                WorldUpTransform = lookAtConstraint.worldUpObject;
            }

            public LookAtExtraData(VRCConstraintBase constraint)
            {
                if (constraint is not VRCLookAtConstraint lookAtConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(VRCLookAtConstraint)}");

                RotationAtRest = lookAtConstraint.RotationAtRest;
                RotationOffset = lookAtConstraint.RotationOffset;
                Roll = lookAtConstraint.Roll;
                UseUpTransform = lookAtConstraint.UseUpTransform;
                WorldUpTransform = lookAtConstraint.WorldUpTransform;
            }
        }

        private class AimExtraData : IConstraintExtraData
        {
            public readonly bool AffectsRotationX;
            public readonly bool AffectsRotationY;
            public readonly bool AffectsRotationZ;
            public readonly Vector3 AimAxis;
            public readonly Vector3 RotationAtRest;
            public readonly Vector3 RotationOffset;
            public readonly Vector3 UpAxis;
            public readonly VRCConstraintBase.WorldUpType WorldUp;
            public readonly Transform WorldUpTransform;
            public readonly Vector3 WorldUpVector;

            public AimExtraData(IConstraint constraint)
            {
                if (constraint is not AimConstraint aimConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(AimConstraint)}");

                RotationAtRest = aimConstraint.rotationAtRest;
                RotationOffset = aimConstraint.rotationOffset;
                AffectsRotationX = (aimConstraint.rotationAxis & Axis.X) != 0;
                AffectsRotationY = (aimConstraint.rotationAxis & Axis.Y) != 0;
                AffectsRotationZ = (aimConstraint.rotationAxis & Axis.Z) != 0;
                WorldUp = (VRCConstraintBase.WorldUpType)(int)aimConstraint.worldUpType;
                WorldUpTransform = aimConstraint.worldUpObject;
                WorldUpVector = aimConstraint.worldUpVector;
                AimAxis = aimConstraint.aimVector;
                UpAxis = aimConstraint.upVector;
            }

            public AimExtraData(VRCConstraintBase constraint)
            {
                if (constraint is not VRCAimConstraint aimConstraint)
                    throw new ArgumentException($"Constraint must be a {nameof(VRCAimConstraint)}");

                RotationAtRest = aimConstraint.RotationAtRest;
                RotationOffset = aimConstraint.RotationOffset;
                AffectsRotationX = aimConstraint.AffectsRotationX;
                AffectsRotationY = aimConstraint.AffectsRotationY;
                AffectsRotationZ = aimConstraint.AffectsRotationZ;
                WorldUp = aimConstraint.WorldUp;
                WorldUpTransform = aimConstraint.WorldUpTransform;
                WorldUpVector = aimConstraint.WorldUpVector;
                AimAxis = aimConstraint.AimAxis;
                UpAxis = aimConstraint.UpAxis;
            }
        }

        private enum ConstraintType
        {
            Position,
            Rotation,
            Scale,
            Parent,
            LookAt,
            Aim
        }
    }
}
#endif