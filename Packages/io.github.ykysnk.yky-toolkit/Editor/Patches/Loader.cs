using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.HarmonyUtilities;
using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.ykyToolkit.Editor.Patches;
using UnityEngine;

[assembly: ExportsPatchLoader(typeof(Loader))]

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class Loader : PatchLoader<Loader>
    {
        internal static readonly MethodInfo MaterialCheckMethod = AccessTools.Method(ThisType, nameof(MaterialCheck));
        public override string QualifiedName => "io.github.ykysnk.yky-toolkit.patches";
        public override string DisplayName => "YKY Toolkit Patches";

        public override void Load()
        {
#if YKYTOOLKIT_LILEDITORTOOLBOX
            MaterialQueuePatch.Instance.Run();
            MaterialShaderPatch.Instance.Run();
            MaterialVariantPatch.Instance.Run();
#endif
        }

        private static bool MaterialCheck(Material material) => material;

        internal static IEnumerable<CodeInstruction> MaterialCheckTranspiler(IEnumerable<CodeInstruction> instructions,
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