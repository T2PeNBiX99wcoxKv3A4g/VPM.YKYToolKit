using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.ykyToolkit.Editor.Patches;

// The editor UI of this package localizes through this id; the S()/G()/UILocalize() extensions resolve the helper
// from it, so the package needs no wrapper class of its own.
[assembly: LocalizationPackage("io.github.ykysnk.yky-toolkit")]

// The patches this package applies to other packages, so a plugin that is installed but broken still behaves.
[assembly: ExportsPatchLoader(typeof(LilEditorToolboxPatches))]