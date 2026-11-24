using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.ykyToolkit.Editor.Patches;
using UnityEngine;
#if YKYTOOLKIT_LILEDITORTOOLBOX
using jp.lilxyzw.editortoolbox;
#endif

[assembly: ExportsPatchLoader(typeof(Loader))]

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class Loader : PatchLoader<Loader>
    {
        public override string QualifiedName => "io.github.ykysnk.yky-toolkit.patches";
        public override string DisplayName => "YKY Toolkit Patches";

        public override void Load(Harmony harmony)
        {
#if YKYTOOLKIT_LILEDITORTOOLBOX
            Run(MaterialQueuePatch.Instance);
            Run(MaterialShaderPatch.Instance);
            Run(MaterialVariantPatch.Instance);
#endif
        }

#if YKYTOOLKIT_LILEDITORTOOLBOX
        internal static bool MaterialCheckPrefix(string guid, string extension) => !ProjectExtension.isIconGUI &&
            extension == ".mat" && ProjectExtension.GUIDToObject(guid) is Material material && material;
#endif
    }
}