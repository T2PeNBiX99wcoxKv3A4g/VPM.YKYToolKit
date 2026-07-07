#if YKYTOOLKIT_LILEDITORTOOLBOX
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class MaterialVariantPatch : Patch<MaterialVariantPatch>
    {
        private static readonly Type MaterialVariantType =
            AccessTools.TypeByName("jp.lilxyzw.editortoolbox.MaterialVariant");

        protected override void Execute()
        {
        }

        [HarmonyPatch]
        [PublicAPI]
        private static class OnGUI
        {
            private static readonly MethodInfo Method = AccessTools.Method(MaterialVariantType, nameof(OnGUI));

            private static MethodBase TargetMethod() => Method;

            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                ILGenerator il) => Loader.MaterialCheckTranspiler(instructions, il);
        }
    }
}
#endif