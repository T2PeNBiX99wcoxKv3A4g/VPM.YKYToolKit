using System;
using System.Reflection;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
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
            AccessTools.TypeByName("jp.lilxyzw.editortoolbox.ProjectExtension");

        private static readonly PropertyInfo? IconGuiProperty =
            ProjectExtensionType == null ? null : AccessTools.Property(ProjectExtensionType, "isIconGUI");

        private static readonly MethodInfo? GuidToObjectMethod = ProjectExtensionType == null
            ? null
            : AccessTools.Method(ProjectExtensionType, "GUIDToObject", new[]
            {
                typeof(string)
            });

        public override string QualifiedName => "io.github.ykysnk.yky-toolkit.patches";

        public override string DisplayName => "YKY Toolkit Patches";

        public override void Load(Harmony harmony)
        {
            // Without the plugin nothing is patched; the patch classes only exist when YKYTOOLKIT_LILEDITORTOOLBOX is set.
#if YKYTOOLKIT_LILEDITORTOOLBOX
            if (IconGuiProperty == null || GuidToObjectMethod == null) return;

            Run(MaterialQueuePatch.Instance);
            Run(MaterialShaderPatch.Instance);
            Run(MaterialVariantPatch.Instance);
#endif
        }

        internal static bool MaterialCheckPrefix(string guid, string extension) =>
            extension == ".mat" &&
            IconGuiProperty?.GetValue(null) is false &&
            GuidToObjectMethod?.Invoke(null, new object[]
            {
                guid
            }) is Material material &&
            material;
    }
}