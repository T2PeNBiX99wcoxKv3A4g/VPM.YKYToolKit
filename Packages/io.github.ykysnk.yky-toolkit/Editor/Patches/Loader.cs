using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.ykyToolkit.Editor.Patches;
using jp.lilxyzw.editortoolbox;
using UnityEngine;

[assembly: ExportsPatchLoader(typeof(Loader))]

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    internal class Loader : PatchLoader<Loader>
    {
        public override string QualifiedName => "io.github.ykysnk.yky-toolkit.patches";
        public override string DisplayName => "YKY Toolkit Patches";

        public override void Load()
        {
#if YKYTOOLKIT_LILEDITORTOOLBOX
            Run(MaterialQueuePatch.Instance);
            Run(MaterialShaderPatch.Instance);
            Run(MaterialVariantPatch.Instance);
#endif
        }

        internal static bool MaterialCheckPrefix(string guid, string extension) => !ProjectExtension.isIconGUI &&
            extension == ".mat" && ProjectExtension.GUIDToObject(guid) is Material material && material;
    }
}