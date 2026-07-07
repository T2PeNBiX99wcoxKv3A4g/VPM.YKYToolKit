#if YKYTOOLKIT_LILEDITORTOOLBOX
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor.HarmonyUtilities;
using io.github.ykysnk.utils.Editor.Patches;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class MaterialQueuePatch : Patch<MaterialQueuePatch>
    {
        private static readonly Type MaterialQueueType = AccessTools.TypeByName("jp.lilxyzw.editortoolbox.MaterialQueue");

        private static readonly MethodInfo MaterialCheckMethod = AccessTools.Method(ThisType, nameof(MaterialCheck));

        protected override void Execute()
        {
            Utils.Log(nameof(MaterialQueuePatch), "Test2");
        }

        private static bool MaterialCheck(Material material) => material;

        [HarmonyPatch]
        [PublicAPI]
        private static class OnGUI
        {
            private static readonly MethodInfo Method = AccessTools.Method(MaterialQueueType, nameof(OnGUI));

            private static MethodBase TargetMethod() => Method;

            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                ILGenerator il)
            {
                var found = false;

                using var cursor = new CodeCursor(instructions);

                while (cursor.MoveNext())
                {
                    yield return cursor.Current!;

                    if (found || cursor.Current!.opcode != OpCodes.Ret) continue;
                    found = true;

                    var label = il.DefineLabel();

                    cursor.Next?.labels.Add(label);

                    yield return new(OpCodes.Ldloc_0);
                    yield return new(OpCodes.Call, MaterialCheckMethod);
                    yield return new(OpCodes.Brfalse_S, label);
                    yield return new(OpCodes.Ret);
                }
            }
        }
    }
}
#endif