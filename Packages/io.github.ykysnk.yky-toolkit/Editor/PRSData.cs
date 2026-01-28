using System;
using io.github.ykysnk.utils.Extensions;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    public struct PRSData
    {
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;

        public PRSData(Vector3 copyPosition, Vector3 copyEulerAngles, Vector3 copyScale)
        {
            position = copyPosition.Clean();
            eulerAngles = copyEulerAngles.Clean().DeltaAngle();
            scale = copyScale.Clean();
        }
    }
}