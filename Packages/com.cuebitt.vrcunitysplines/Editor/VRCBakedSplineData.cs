using UnityEngine;

namespace Cuebitt.VRCUnitySplines
{
    // Editor-only intermediate: holds a baked spline so clip and scatter
    // bakers can reuse it without re-evaluating the live spline. Udon never
    // touches this type, Udon only sees plain arrays copied onto components
    // by the bake window. One asset per baked spline, stored in the source
    // SplineContainer's local space.
    [CreateAssetMenu(menuName = "VRCUnitySplines/Baked Spline", fileName = "BakedSpline")]
    public class VRCBakedSplineData : ScriptableObject
    {
        // one entry per resampled frame, all arrays line up by index
        [Tooltip("Resampled positions in source-container local space.")]
        public Vector3[] positions = new Vector3[0];

        [Tooltip("Normalized tangents matching positions.")]
        public Vector3[] tangents = new Vector3[0];

        [Tooltip("Up vectors matching positions.")]
        public Vector3[] upVectors = new Vector3[0];

        // arc length lookup, lets followers move at constant speed
        [Tooltip("Cumulative arc length at each frame. Same length as positions.")]
        public float[] cumulativeLengths = new float[0];

        [Tooltip("Total baked length in local units.")]
        public float totalLength;

        // bookkeeping from bake time
        [Tooltip("True when the source spline was closed.")]
        public bool closed;

        [Tooltip("Human-readable bake source, e.g. 'Road/SplineContainer [1] @32/curve'.")]
        public string sourceDescription = "";
    }
}
