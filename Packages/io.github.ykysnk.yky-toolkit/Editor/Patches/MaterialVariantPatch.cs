#if YKYTOOLKIT_LILEDITORTOOLBOX
using System;
using System.Reflection;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
using JetBrains.Annotations;
using UnityEngine;

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

            private static bool Prefix(ref Rect currentRect, string guid, string path, string name, string extension,
                Rect fullRect) => Loader.MaterialCheckPrefix(guid, extension);
        }
    }
}
#endif