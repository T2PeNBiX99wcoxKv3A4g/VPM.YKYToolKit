using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.ykyToolkit.Editor.Patches;

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
            MaterialQueuePatch.Instance.Run();
#endif
        }
    }
}