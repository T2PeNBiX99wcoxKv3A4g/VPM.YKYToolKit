using System;
using System.Reflection;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.ykyToolkit.Editor.Patches;
using UnityEngine;

[assembly: ExportsPatchLoader(typeof(Loader))]

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class Loader : PatchLoader<Loader>
    {
        // The lilEditorToolbox integration is optional: the plugin is reached through reflection instead of an
        // assembly reference, so installing this package without the plugin still compiles.
        private static readonly Type? ProjectExtensionType =
            ReflectionUtils.FindType("jp.lilxyzw.editortoolbox.ProjectExtension");

        private static readonly MethodInfo? GuidToObjectMethod = ProjectExtensionType == null
            ? null
            : ReflectionUtils.FindMethod(ProjectExtensionType, "GUIDToObject");

        public override string QualifiedName => "io.github.ykysnk.yky-toolkit.patches";

        public override string DisplayName => "YKY Toolkit Patches";

        public override void Load(Harmony harmony)
        {
            // Without the plugin nothing is patched; the patch classes only exist when YKYTOOLKIT_LILEDITORTOOLBOX is set.
#if YKYTOOLKIT_LILEDITORTOOLBOX
            if (GuidToObjectMethod == null) return;

            Run(MaterialQueuePatch.Instance);
            Run(MaterialShaderPatch.Instance);
            Run(MaterialVariantPatch.Instance);
#endif
        }

        // The toolbox caches GUIDToObject results. A cached object compares equal to null after Unity destroys it, so
        // this check is what keeps its component from reading a destroyed material and breaking the project window.
        internal static bool MaterialCheckPrefix(string guid, string extension) =>
            extension == ".mat" &&
            GuidToObjectMethod?.Invoke(null, new object[]
            {
                guid
            }) is Material material &&
            material;
    }
}
