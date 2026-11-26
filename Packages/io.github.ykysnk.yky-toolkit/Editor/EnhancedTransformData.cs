using System;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    public class EnhancedTransformData
    {
        public string id;
        public int positionDecimalPrecision = -1;
        public int rotationDecimalPrecision = -1;
        public int scaleDecimalPrecision = -1;
        public bool lockTransform;

        public EnhancedTransformData(Object obj) => id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();

        public EnhancedTransformData(string id) => this.id = id;

        /// <summary>Object the stored id points at, or null when it was deleted or the id cannot be parsed.</summary>
        internal static Object? Resolve(string id)
        {
            if (string.IsNullOrEmpty(id) || !GlobalObjectId.TryParse(id, out var globalId)) return null;
            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
        }

        /// <summary>Hierarchy path for the row; the raw GlobalObjectId is not something a user can read.</summary>
        internal static string DisplayName(string id)
        {
            var obj = Resolve(id);

            return obj switch
            {
                null => "label.enhanced_transform_database.missing".S(),
                Transform transform => transform.FullName(),
                GameObject gameObject => gameObject.transform.FullName(),
                _ => obj.name
            };
        }
    }
}
