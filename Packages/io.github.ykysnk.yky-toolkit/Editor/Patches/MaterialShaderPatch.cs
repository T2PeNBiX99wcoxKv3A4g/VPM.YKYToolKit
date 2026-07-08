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
    internal class MaterialShaderPatch : Patch<MaterialShaderPatch>
    {
        private static readonly Type TargetType = AccessTools.TypeByName("jp.lilxyzw.editortoolbox.MaterialShader");

        protected override void Execute()
        {
        }

        [UsedImplicitly]
        private class OnGUI : PatchMethod<OnGUI>
        {
            public override MethodInfo? TargetMethod { get; } = AccessTools.Method(TargetType, nameof(OnGUI));
            public override MethodInfo? TargetPrefix { get; } = Method(nameof(Prefix));

            [SuppressMessage("ReSharper", "UnusedParameter.Local")]
            private static bool Prefix(ref Rect currentRect, string guid, string path, string name, string extension,
                Rect fullRect) => Loader.MaterialCheckPrefix(guid, extension);
        }
    }
}
#endif