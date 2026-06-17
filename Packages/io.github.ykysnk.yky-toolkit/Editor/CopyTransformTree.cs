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
            var result = new List<TransformTreeData>();
            var stack = new Stack<(string path, Transform transform)>();

            stack.Push(new("", copyObject.transform));

            while (stack.Count > 0)
            {
                var (path, current) = stack.Pop();

                result.Add(new(path, current));

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
            if (JsonUtils.TryFromJson<ListWrapper<TransformTreeData>>(copyDataJson, out var copyData, out var exception))
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var data in copyData!)
                {
                    var found = string.IsNullOrEmpty(data.path)
                        ? pasteObject.transform
                        : pasteObject.transform.Find(data.path);
                    if (found == null)
                    {
                        Utils.LogWarning(nameof(CopyTransformTree),
                            $"Paste failed: {pasteObject.name} does not have a child named {data.path}");
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
            public string path;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;

            public TransformTreeData(string path, Transform transform)
            {
                this.path = path;
                position = transform.localPosition;
                rotation = transform.localEulerAngles;
                scale = transform.localScale;
            }
        }
    }
}