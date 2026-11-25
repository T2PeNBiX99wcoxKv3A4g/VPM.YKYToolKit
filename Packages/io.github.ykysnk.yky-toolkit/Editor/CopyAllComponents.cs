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
        private const string UndoName = "Paste All Components";

        [MenuItem("GameObject/YKYToolkit/Copy All Components", false, Util.Three)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Copy All Components", false, Util.One2)]
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
        [MenuItem("CONTEXT/Component/YKYToolkit/Paste All Components", false, Util.One2)]
        private static void Paste()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var pasteObject = selectedObjects[0];
            var copyDataJson = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(copyDataJson)) return;
            PasteAsync(copyDataJson, pasteObject, false).Forget();
        }

        [MenuItem("GameObject/YKYToolkit/Paste All Components With Transform", false, Util.Three)]
        [MenuItem("CONTEXT/Component/YKYToolkit/Paste All Components With Transform", false, Util.One2)]
        private static void PasteWithTransform()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length < 1) return;

            var pasteObject = selectedObjects[0];
            var copyDataJson = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(copyDataJson)) return;
            PasteAsync(copyDataJson, pasteObject, true).Forget();
        }

        private static async UniTask PasteAsync(string copyDataJson, GameObject pasteObject, bool includeTransform)
        {
            if (!JsonUtils.TryFromJson<ListWrapper<ComponentData>>(copyDataJson, out var copyData, out var exception))
            {
                Utils.LogError(nameof(CopyAllComponents),
                    $"Paste failed: {exception!.Message}\n{exception.StackTrace}");
                return;
            }

            // The Transform is the first entry that ComponentsSelect produced; pasting without it starts after that.
            var start = includeTransform ? 0 : 1;
            if (copyData!.Count <= start) return;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            var stopwatch = Stopwatch.StartNew();

            for (var i = start; i < copyData.Count; i++)
            {
                var componentData = copyData[i];
                if (!TryResolveComponent(pasteObject, i, componentData, out var component)) continue;

                Undo.RecordObject(component, UndoName);

                Try.Run(() => EditorJsonUtility.FromJsonOverwrite(componentData.componentJson, component))
                    .OnFailure(ex =>
                        Utils.LogError(nameof(CopyAllComponents),
                            $"Overwrite component failed: {ex!.Message}\n{ex.StackTrace}"));

                if (stopwatch.ElapsedMilliseconds <= Util.StopwatchWaitElapsedMilliseconds) continue;
                await UniTask.Yield();
                stopwatch.Restart();
            }
        }

        private static bool TryResolveComponent(GameObject target, int index, ComponentData componentData,
            out Component? component)
        {
            // Matching by type keeps the paste correct when the component order differs from the source object; the
            // index is only a fallback for entries without a type name.
            var type = string.IsNullOrEmpty(componentData.componentAssemblyQualifiedName)
                ? null
                : Type.GetType(componentData.componentAssemblyQualifiedName);

            if (type == null) return target.TryGetComponentAtIndex(index, out component);

            if (!target.TryGetComponent(type, out component))
                component = Undo.AddComponent(target, type);

            return component != null;
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
