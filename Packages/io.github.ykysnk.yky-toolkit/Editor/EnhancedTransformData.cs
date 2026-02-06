using System;
using UnityEditor;
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
    }
}