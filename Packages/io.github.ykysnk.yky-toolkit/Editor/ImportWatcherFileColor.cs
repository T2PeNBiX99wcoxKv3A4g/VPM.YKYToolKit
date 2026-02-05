using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class ImportWatcherFileColor : IEquatable<ImportWatcherFileColor>
    {
        public static readonly ImportWatcherFileColor[] DefaultColors =
        {
            new(".cs", new(1f, 0.85f, 0.2f, 0.45f)), new(".prefab", new(0.4f, 0.6f, 1f, 0.45f)),
            new(".png", new(0.4f, 1f, 0.4f, 0.45f)), new(".jpg", new(0.4f, 1f, 0.4f, 0.45f)),
            new(".tga", new(0.4f, 1f, 0.4f, 0.45f)), new(".mat", new(0.4f, 1f, 0.4f, 0.45f)),
            new(".anim", new(0.4f, 1f, 0.4f, 0.45f)), new(".uxml", new(1f, 0.2f, 0.2f, 0.45f)),
            new(".uss", new(1f, 0.2f, 0.2f, 0.45f))
        };

        public static readonly Color DefaultColor = new(1, 1, 1, 0.45f);
        public string fileExtension;
        public Color color;

        public ImportWatcherFileColor(string fileExtension, Color color)
        {
            this.fileExtension = fileExtension;
            this.color = color;
        }

        public bool Equals(ImportWatcherFileColor? other)
        {
            if (other is null)
                return false;
            return fileExtension == other.fileExtension;
        }

        public override bool Equals(object? obj) => Equals(obj as ImportWatcherFileColor);
        public override int GetHashCode() => fileExtension.GetHashCode();

        public override string ToString() =>
            $"{nameof(ImportWatcherFileColor)}(fileExtension: {fileExtension}, color: {color})";
    }
}