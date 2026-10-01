using System;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // In-memory bake result, plain data with no asset behind it. The bake
    // window holds one of these and feeds it to whichever output is pressed.
    // Serializable so the window keeps it across script recompiles.
    [Serializable]
    public class BakedSpline
    {
        // all arrays are index-aligned
        public Vector3[] positions = new Vector3[0];

        public Vector3[] tangents = new Vector3[0];

        public Vector3[] upVectors = new Vector3[0];

        public float[] cumulativeLengths = new float[0];

        public float totalLength;

        public bool closed;

        public string sourceDescription = "";
    }
}
