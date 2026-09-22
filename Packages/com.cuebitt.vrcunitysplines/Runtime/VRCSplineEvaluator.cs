using UdonSharp;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines
{
    // Opt-in only. Prefer the baked AnimationClip (default) or the VRCTween
    // driver (fallback): both run natively. This behaviour exists for cases
    // that need runtime control, like scrubbing or distance queries.
    // Data lives in plain arrays, filled by the bake window, because Udon
    // cannot read custom asset types at runtime.
    // ponytail: linear scan over baked frames; fine for a few hundred frames,
    // switch to a binary search if bake densities ever grow past that.
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRCSplineEvaluator : UdonSharpBehaviour
    {
        [Tooltip("Baked positions in source-container local space. Fill from the bake window.")]
        public Vector3[] positions = new Vector3[0];

        [Tooltip("Normalized tangents matching positions.")]
        public Vector3[] tangents = new Vector3[0];

        [Tooltip("Up vectors matching positions.")]
        public Vector3[] upVectors = new Vector3[0];

        [Tooltip("Cumulative arc length per frame, same length as positions.")]
        public float[] cumulativeLengths = new float[0];

        [Tooltip("Total baked length in local units.")]
        public float totalLength;

        [Tooltip("True when the source spline was a closed loop.")]
        public bool closed;

        [Tooltip("Object to move. Defaults to this transform.")]
        public Transform target;

        [Tooltip("Advance in Update. Off by default; call the setters instead.")]
        public bool driveEveryFrame;

        [Tooltip("Traversal speed in local units per second when driving.")]
        public float speed = 1f;

        private float _distance;

        void Start()
        {
            // fall back to our own transform when no target set
            if (target == null) target = transform;
            ApplyDistance(_distance);
        }

        void Update()
        {
            // opt-in only, off unless someone asked for it
            if (!driveEveryFrame || !HasData() || totalLength <= 0f) return;

            _distance += speed * Time.deltaTime;

            // wrap closed loops, clamp open ones
            if (closed) _distance %= totalLength;
            else _distance = Mathf.Min(_distance, totalLength);

            ApplyDistance(_distance);
        }

        public void SetNormalizedTime(float t)
        {
            if (!HasData()) return;
            ApplyDistance(Mathf.Clamp01(t) * totalLength);
        }

        public void SetDistance(float distance)
        {
            ApplyDistance(distance);
        }

        public Vector3 GetPositionAt(float normalizedTime)
        {
            // read-only query, never touches the target
            if (!HasData()) return transform.position;
            return SamplePosition(Mathf.Clamp01(normalizedTime) * totalLength);
        }

        private void ApplyDistance(float distance)
        {
            if (!HasData() || target == null) return;
            _distance = distance;

            // sample each channel then pose the target
            Vector3 pos = SamplePosition(distance);
            Vector3 tan = SampleDirection(tangents, distance);
            Vector3 up = SampleDirection(upVectors, distance);

            target.localPosition = pos;

            // skip rotation when the tangent collapsed
            if (tan.sqrMagnitude > 1e-8f)
                target.localRotation = Quaternion.LookRotation(tan, up.sqrMagnitude > 1e-8f ? up : Vector3.up);
        }

        private bool HasData()
        {
            return positions != null && positions.Length >= 2
                && cumulativeLengths != null && cumulativeLengths.Length >= 2;
        }

        private int FindFrame(float distance)
        {
            // first frame at or past the distance
            for (int i = 1; i < cumulativeLengths.Length; i++)
                if (cumulativeLengths[i] >= distance) return i;
            return cumulativeLengths.Length - 1;
        }

        private Vector3 SamplePosition(float distance)
        {
            // blend between the two bracketing frames
            int i = FindFrame(distance);
            float span = cumulativeLengths[i] - cumulativeLengths[i - 1];
            float f = span > 1e-9f ? (distance - cumulativeLengths[i - 1]) / span : 0f;
            return Vector3.Lerp(positions[i - 1], positions[i], f);
        }

        private Vector3 SampleDirection(Vector3[] vectors, float distance)
        {
            // same blend as positions, then renormalize
            int i = FindFrame(distance);
            float span = cumulativeLengths[i] - cumulativeLengths[i - 1];
            float f = span > 1e-9f ? (distance - cumulativeLengths[i - 1]) / span : 0f;
            return Vector3.Lerp(vectors[i - 1], vectors[i], f).normalized;
        }
    }
}
