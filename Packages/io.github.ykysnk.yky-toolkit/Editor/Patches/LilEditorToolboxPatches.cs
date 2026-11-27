using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using io.github.ykysnk.utils.Editor.Patches;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor.Patches
{
    /// <summary>
    ///     Patches for lilEditorToolbox. Its project extension caches every object it loaded by GUID, so deleting an
    ///     asset leaves a destroyed wrapper in the cache and the components that draw it throw a
    ///     <c>MissingReferenceException</c> on each repaint of the Project window.
    /// </summary>
    internal sealed class LilEditorToolboxPatches : PatchLoader<LilEditorToolboxPatches>
    {
        public override string QualifiedName => "io.github.ykysnk.yky-toolkit.lil-editor-toolbox";
        public override string DisplayName => "YKY Toolkit / lilEditorToolbox";

        public override void Load(Harmony harmony) => Run(ProjectExtensionCache.Instance);

        /// <summary>
        ///     Fixes the cache instead of one component, so the queue, shader and variant labels all stop throwing.
        /// </summary>
        internal sealed class ProjectExtensionCache : Patch<ProjectExtensionCache>
        {
            private const string ExtensionTypeName = "jp.lilxyzw.editortoolbox.ProjectExtension";

            /// <summary>The cache is private static, and it only exists when the plugin is installed.</summary>
            private static readonly Lazy<Dictionary<string, Object>?> Cache = new(() =>
                ReflectionUtils.FindType(ExtensionTypeName)?
                    .GetField("objMap", BindingFlags.Static | BindingFlags.NonPublic)?
                    .GetValue(null) as Dictionary<string, Object>);

            /// <summary>The plugin is optional, so the whole group stays off when it is not installed.</summary>
            public override bool Enabled => Cache.Value != null;

            protected override void Execute(Harmony harmony)
            {
                // The group has nothing to apply by hand; the nested patch below does the work.
            }

            [UsedImplicitly]
            private sealed class ForgetDestroyedObject : PatchMethod<ForgetDestroyedObject>
            {
                public override MethodInfo? TargetMethod { get; } =
                    ReflectionUtils.FindType(ExtensionTypeName)?.GetMethod("GUIDToObject",
                        BindingFlags.Public | BindingFlags.Static);

                /// <summary>Hidden with the plugin renamed instead of patched, which is not worth a warning.</summary>
                public override bool Enabled => TargetMethod != null;

                /// <summary>
                ///     Removes an entry whose object was destroyed, so the original loads the asset again. A deleted
                ///     asset then resolves to <see langword="null" /> instead of a wrapper that throws when it is read.
                /// </summary>
                private static void Prefix(string guid)
                {
                    var cache = Cache.Value;
                    if (cache == null) return;
                    if (!cache.TryGetValue(guid, out var cached) || ReferenceEquals(cached, null) || cached) return;

                    cache.Remove(guid);
                }
            }
        }
    }
}