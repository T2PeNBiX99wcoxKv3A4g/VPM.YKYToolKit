using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>Window icons shipped in <c>Editor/Icons</c>, next to the SVG sources.</summary>
    internal static class ToolkitIcons
    {
        private const string Folder = "Packages/io.github.ykysnk.yky-toolkit/Editor/Icons/";

        /// <summary>Loads one icon by file name, without the extension.</summary>
        [CanBeNull]
        public static Texture2D Load(string name) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + name + ".png");

        /// <summary>Window title content with the icon, or the plain text when the icon is missing.</summary>
        [NotNull]
        public static GUIContent Content(string text, string name)
        {
            var texture = Load(name);
            return texture == null ? new GUIContent(text) : new GUIContent(text, texture);
        }
    }
}
