using System;
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
        [MenuItem("GameObject/YKYToolkit/Copy All Components")]
        [MenuItem("CONTEXT/Component/YKYToolkit/Copy All Components")]
        private static void Copy()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var copyObject = selectedObjects[0];
            var componentDatas = copyObject.ComponentsSelect((_, component) => new ComponentData(component));
            var copyData = new CopyData(componentDatas);

            if (JsonUtils.TryToJson(copyData, out var json, out var exception))
                EditorGUIUtility.systemCopyBuffer = json;
            else
                Utils.LogWarning(nameof(CopyAllComponents),
                    $"Failed to copy components. {exception!.Message}\n{exception.StackTrace}");
        }

        [MenuItem("GameObject/YKYToolkit/Paste All Components")]
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

        [MenuItem("GameObject/YKYToolkit/Paste All Components With Transform")]
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
            if (JsonUtils.TryFromJson<CopyData>(copyDataJson, out var copyData, out var exception))
            {
                if (copyData.componentDatas.Length < 2) return;

                for (var i = 1; i < copyData.componentDatas.Length; i++)
                {
                    var componentData = copyData.componentDatas[i];

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

                    await UniTask.DelayFrame(10);
                }
            }
            else
                Utils.LogError(nameof(CopyAllComponents), $"Paste failed: {exception!.Message}\n{exception.StackTrace}");
        }

        private static async UniTask PasteAsyncWithTransform(string copyDataJson, GameObject pasteObject)
        {
            if (JsonUtils.TryFromJson<CopyData>(copyDataJson, out var copyData, out var exception))
                for (var i = 0; i < copyData.componentDatas.Length; i++)
                {
                    var componentData = copyData.componentDatas[i];

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

                    await UniTask.DelayFrame(10);
                }
            else
                Utils.LogError(nameof(CopyAllComponents), $"Paste failed: {exception!.Message}\n{exception.StackTrace}");
        }

        [Serializable]
        private struct CopyData
        {
            public ComponentData[] componentDatas;

            public CopyData(ComponentData[] componentDatas) => this.componentDatas = componentDatas;
        }

        [Serializable]
        private struct ComponentData
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