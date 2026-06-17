using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class CopyTransformTree
    {
        [MenuItem("GameObject/YKYToolkit/Copy Transform Tree", false, Util.Four)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Copy Transform Tree", false, Util.Twe2)]
        private static void Copy()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var copyObject = selectedObjects[0];
            var result = new Dictionary<string, TransformTreeData>();
            var stack = new Stack<(string path, Transform transform)>();

            stack.Push(new("", copyObject.transform));

            while (stack.Count > 0)
            {
                var (path, current) = stack.Pop();

                result.TryAdd(path, new(current));

                foreach (Transform child in current)
                    stack.Push(new(string.IsNullOrEmpty(path) ? child.name : $"{path}/{child.name}", child));
            }

            if (JsonUtils.TryToJson(Wrapper.Create(result), out var json, out var exception))
                EditorGUIUtility.systemCopyBuffer = json;
            else
                Utils.LogWarning(nameof(CopyTransformTree),
                    $"Failed to copy transform tree. {exception!.Message}\n{exception.StackTrace}");
        }

        [MenuItem("GameObject/YKYToolkit/Paste Transform Tree", false, Util.Four)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Paste Transform Tree", false, Util.Twe2)]
        private static void Paste()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var pasteObject = selectedObjects[0];
            var copyDataJson = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(copyDataJson)) return;
            PasteAsync(copyDataJson, pasteObject).Forget();
        }

        private static async UniTask PasteAsync(string copyDataJson, GameObject pasteObject)
        {
            if (JsonUtils.TryFromJson<DictionaryWrapper<string, TransformTreeData>>(copyDataJson, out var copyData,
                    out var exception))
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var (path, data) in copyData!)
                {
                    var found = string.IsNullOrEmpty(path)
                        ? pasteObject.transform
                        : pasteObject.transform.Find(path);
                    if (found == null)
                    {
                        Utils.LogWarning(nameof(CopyTransformTree),
                            $"Paste failed: {pasteObject.name} does not have a child named {path}");
                        continue;
                    }

                    found.localPosition = data.position;
                    found.localRotation = Quaternion.Euler(data.rotation);
                    found.localScale = data.scale;

                    if (stopwatch.ElapsedMilliseconds <= 30) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }
            }
            else
                Utils.LogError(nameof(CopyTransformTree), $"Paste failed: {exception!.Message}\n{exception.StackTrace}");
        }

        [Serializable]
        private class TransformTreeData
        {
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;

            public TransformTreeData(Transform transform)
            {
                position = transform.localPosition;
                rotation = transform.localEulerAngles;
                scale = transform.localScale;
            }
        }
    }
}