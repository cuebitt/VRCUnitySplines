using UnityEngine;

namespace Cuebitt.VRCUnitySplines
{
    // Plain data holder with no Unity Splines references, so it survives
    // VRChat uploads. One asset per baked spline, stored in the source
    // SplineContainer's local space. Keep baked followers under the same
    // transform and everything lines up even if the parent moves.
    [CreateAssetMenu(menuName = "VRCUnitySplines/Baked Spline", fileName = "BakedSpline")]
    public class VRCBakedSplineData : ScriptableObject
    {
        [Tooltip("Resampled positions in source-container local space.")]
        public Vector3[] positions = new Vector3[0];

        [Tooltip("Normalized tangents matching positions.")]
        public Vector3[] tangents = new Vector3[0];

        [Tooltip("Up vectors matching positions.")]
        public Vector3[] upVectors = new Vector3[0];

        [Tooltip("Cumulative arc length at each frame. Same length as positions.")]
        public float[] cumulativeLengths = new float[0];

        [Tooltip("Total baked length in local units.")]
        public float totalLength;

        [Tooltip("True when the source spline was closed.")]
        public bool closed;

        [Tooltip("Human-readable bake source, e.g. 'Road/SplineContainer [1] @32/curve'.")]
        public string sourceDescription = "";
    }
}
