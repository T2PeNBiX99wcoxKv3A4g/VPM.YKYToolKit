using System;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    public class ImportWatcherFileColor
    {
        public static readonly ImportWatcherFileColor[] DefaultColors =
        {
            new(".cs", new(1f, 0.85f, 0.2f, 0.45f)), new(".prefab", new(0.4f, 0.6f, 1f, 0.45f)),
            new(".png", new(0.4f, 1f, 0.4f, 0.45f)), new(".jpg", new(0.4f, 1f, 0.4f, 0.45f)),
            new(".tga", new(0.4f, 1f, 0.4f, 0.45f)), new(".mat", new(0.4f, 1f, 0.4f, 0.45f)),
            new(".anim", new(0.4f, 1f, 0.4f, 0.45f))
        };

        public static readonly Color DefaultColor = new(1, 1, 1, 0.45f);
        public string fileExtension;
        public Color color;

        public ImportWatcherFileColor(string fileExtension, Color color)
        {
            this.fileExtension = fileExtension;
            this.color = color;
        }
    }
}