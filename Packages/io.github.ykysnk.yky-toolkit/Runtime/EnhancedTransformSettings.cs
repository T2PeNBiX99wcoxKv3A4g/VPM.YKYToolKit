using UnityEngine;

namespace io.github.ykysnk.ykyToolkit
{
    /// <summary>
    ///     Extra transform settings that belong to one object. The Transform inspector adds this component when a
    ///     value becomes non-default and removes it again when everything is back to the default, so it only exists
    ///     where it is needed.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class EnhancedTransformSettings : MonoBehaviour
    {
        public int positionDecimalPrecision = -1;
        public int rotationDecimalPrecision = -1;
        public int scaleDecimalPrecision = -1;
        public bool lockTransform;

        /// <summary>True when every value is the default and the component can be removed again.</summary>
        public bool IsDefault =>
            positionDecimalPrecision == -1 &&
            rotationDecimalPrecision == -1 &&
            scaleDecimalPrecision == -1 &&
            !lockTransform;
    }
}
