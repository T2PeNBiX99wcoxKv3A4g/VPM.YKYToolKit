using System;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Extensions;
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
            EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(copyData);
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
            try
            {
                var copyData = JsonUtility.FromJson<CopyData>(copyDataJson);

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

                    EditorJsonUtility.FromJsonOverwrite(componentData.componentJson, component);
                    await UniTask.Delay(100);
                }
            }
            catch (Exception e)
            {
                Utils.LogError(nameof(CopyAllComponents), $"Paste failed: {e}\n{e.Message}");
            }
        }

        private static async UniTask PasteAsyncWithTransform(string copyDataJson, GameObject pasteObject)
        {
            try
            {
                var copyData = JsonUtility.FromJson<CopyData>(copyDataJson);

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

                    EditorJsonUtility.FromJsonOverwrite(componentData.componentJson, component);
                    await UniTask.Delay(100);
                }
            }
            catch (Exception e)
            {
                Utils.LogError(nameof(CopyAllComponents), $"Paste failed: {e}\n{e.Message}");
            }
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