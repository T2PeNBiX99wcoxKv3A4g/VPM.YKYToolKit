#if YKYTOOLKIT_LILEDITORTOOLBOX
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class MaterialQueuePatch : Patch<MaterialQueuePatch>
    {
        private static readonly Type TargetType = AccessTools.TypeByName("jp.lilxyzw.editortoolbox.MaterialQueue");

        protected override void Execute(Harmony harmony)
        {
        }

        [UsedImplicitly]
        private class OnGUI : PatchMethod<OnGUI>
        {
            public override MethodInfo? TargetMethod { get; } = AccessTools.Method(TargetType, nameof(OnGUI));
            public override string PrefixMethod => nameof(Prefix);

            [SuppressMessage("ReSharper", "UnusedParameter.Local")]
            private static bool Prefix(ref Rect currentRect, string guid, string path, string name, string extension,
                Rect fullRect) => Loader.MaterialCheckPrefix(guid, extension);
        }
    }
}
#endif