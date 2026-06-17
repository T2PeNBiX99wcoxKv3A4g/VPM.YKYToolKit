using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal static class CopyAllComponents
    {
        [MenuItem("GameObject/YKYToolkit/Copy All Components", false, Util.Three)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Copy All Components")]
        private static void Copy()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var copyObject = selectedObjects[0];
            var componentDatas = copyObject.ComponentsSelect((_, component) => new ComponentData(component));

            if (JsonUtils.TryToJson(Wrapper.Create(componentDatas), out var json, out var exception))
                EditorGUIUtility.systemCopyBuffer = json;
            else
                Utils.LogWarning(nameof(CopyAllComponents),
                    $"Failed to copy components. {exception!.Message}\n{exception.StackTrace}");
        }

        [MenuItem("GameObject/YKYToolkit/Paste All Components", false, Util.Three)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Paste All Components")]
        private static void Paste()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var pasteObject = selectedObjects[0];
            var copyDataJson = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(copyDataJson)) return;
            PasteAsync(copyDataJson, pasteObject).Forget();
        }

        [MenuItem("GameObject/YKYToolkit/Paste All Components With Transform", false, Util.Three)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Paste All Components With Transform")]
        private static void PasteWithTransform()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var pasteObject = selectedObjects[0];
            var copyDataJson = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(copyDataJson)) return;
            PasteAsyncWithTransform(copyDataJson, pasteObject).Forget();
        }

        private static async UniTask PasteAsync(string copyDataJson, GameObject pasteObject)
        {
            if (JsonUtils.TryFromJson<ListWrapper<ComponentData>>(copyDataJson, out var copyData, out var exception))
            {
                if (copyData!.Count < 2) return;

                var stopwatch = Stopwatch.StartNew();

                for (var i = 1; i < copyData.Count; i++)
                {
                    var componentData = copyData[i];

                    if (!pasteObject.TryGetComponentAtIndex(i, out var component))
                    {
                        if (string.IsNullOrEmpty(componentData.componentAssemblyQualifiedName)) continue;
                        var type = Type.GetType(componentData.componentAssemblyQualifiedName);
                        if (type == null) continue;
                        if (!pasteObject.TryGetComponent(type, out component))
                            component = pasteObject.AddComponent(type);
                    }

                    Try.Run(() => EditorJsonUtility.FromJsonOverwrite(componentData.componentJson, component))
                        .OnFailure(ex =>
                            Utils.LogError(nameof(CopyAllComponents),
                                $"Overwrite component failed: {ex!.Message}\n{ex.StackTrace}"));

                    if (stopwatch.ElapsedMilliseconds <= 30) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }
            }
            else
                Utils.LogError(nameof(CopyAllComponents), $"Paste failed: {exception!.Message}\n{exception.StackTrace}");
        }

        private static async UniTask PasteAsyncWithTransform(string copyDataJson, GameObject pasteObject)
        {
            if (JsonUtils.TryFromJson<ListWrapper<ComponentData>>(copyDataJson, out var copyData, out var exception))
            {
                var stopwatch = Stopwatch.StartNew();

                for (var i = 0; i < copyData!.Count; i++)
                {
                    var componentData = copyData[i];

                    if (!pasteObject.TryGetComponentAtIndex(i, out var component))
                    {
                        if (string.IsNullOrEmpty(componentData.componentAssemblyQualifiedName)) continue;
                        var type = Type.GetType(componentData.componentAssemblyQualifiedName);
                        if (type == null) continue;
                        if (!pasteObject.TryGetComponent(type, out component))
                            component = pasteObject.AddComponent(type);
                    }

                    Try.Run(() => EditorJsonUtility.FromJsonOverwrite(componentData.componentJson, component))
                        .OnFailure(ex =>
                            Utils.LogError(nameof(CopyAllComponents),
                                $"Overwrite component failed: {ex!.Message}\n{ex.StackTrace}"));

                    if (stopwatch.ElapsedMilliseconds <= 30) continue;
                    await UniTask.Yield();
                    stopwatch.Restart();
                }
            }
            else
                Utils.LogError(nameof(CopyAllComponents), $"Paste failed: {exception!.Message}\n{exception.StackTrace}");
        }

        [Serializable]
        private class ComponentData
        {
            public string componentAssemblyQualifiedName;
            public string componentJson;

            public ComponentData(Component component)
            {
                componentAssemblyQualifiedName = component.GetType().AssemblyQualifiedName ?? string.Empty;
                componentJson = EditorJsonUtility.ToJson(component);
            }
        }
    }
}