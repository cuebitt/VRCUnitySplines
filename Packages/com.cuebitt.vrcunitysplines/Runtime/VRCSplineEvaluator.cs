using UdonSharp;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines
{
    // Opt-in only. Prefer the baked AnimationClip (default) or the VRCTween
    // driver (fallback): both run natively. This behaviour exists for cases
    // that need runtime control, like scrubbing or distance queries.
    // ponytail: linear scan over baked frames; fine for a few hundred frames,
    // switch to a binary search if bake densities ever grow past that.
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRCSplineEvaluator : UdonSharpBehaviour
    {
        [Tooltip("Baked spline to read from.")]
        public VRCBakedSplineData spline;

        [Tooltip("Object to move. Defaults to this transform.")]
        public Transform target;

        [Tooltip("Advance in Update. Off by default; call the setters instead.")]
        public bool driveEveryFrame;

        [Tooltip("Traversal speed in local units per second when driving.")]
        public float speed = 1f;

        private float _distance;

        void Start()
        {
            if (target == null) target = transform;
            ApplyDistance(_distance);
        }

        void Update()
        {
            if (!driveEveryFrame || spline == null || spline.totalLength <= 0f) return;
            _distance += speed * Time.deltaTime;
            if (spline.closed) _distance %= spline.totalLength;
            else _distance = Mathf.Min(_distance, spline.totalLength);
            ApplyDistance(_distance);
        }

        public void SetNormalizedTime(float t)
        {
            if (spline == null) return;
            ApplyDistance(Mathf.Clamp01(t) * spline.totalLength);
        }

        public void SetDistance(float distance)
        {
            ApplyDistance(distance);
        }

        public Vector3 GetPositionAt(float normalizedTime)
        {
            if (!HasData()) return transform.position;
            return SamplePosition(Mathf.Clamp01(normalizedTime) * spline.totalLength);
        }

        private void ApplyDistance(float distance)
        {
            if (!HasData() || target == null) return;
            _distance = distance;
            Vector3 pos = SamplePosition(distance);
            Vector3 tan = SampleDirection(spline.tangents, distance);
            Vector3 up = SampleDirection(spline.upVectors, distance);
            target.localPosition = pos;
            if (tan.sqrMagnitude > 1e-8f)
                target.localRotation = Quaternion.LookRotation(tan, up.sqrMagnitude > 1e-8f ? up : Vector3.up);
        }

        private bool HasData()
        {
            return spline != null && spline.positions != null && spline.positions.Length >= 2;
        }

        private int FindFrame(float distance)
        {
            float[] lengths = spline.cumulativeLengths;
            for (int i = 1; i < lengths.Length; i++)
                if (lengths[i] >= distance) return i;
            return lengths.Length - 1;
        }

        private Vector3 SamplePosition(float distance)
        {
            int i = FindFrame(distance);
            float[] lengths = spline.cumulativeLengths;
            float span = lengths[i] - lengths[i - 1];
            float f = span > 1e-9f ? (distance - lengths[i - 1]) / span : 0f;
            return Vector3.Lerp(spline.positions[i - 1], spline.positions[i], f);
        }

        private Vector3 SampleDirection(Vector3[] vectors, float distance)
        {
            int i = FindFrame(distance);
            float[] lengths = spline.cumulativeLengths;
            float span = lengths[i] - lengths[i - 1];
            float f = span > 1e-9f ? (distance - lengths[i - 1]) / span : 0f;
            return Vector3.Lerp(vectors[i - 1], vectors[i], f).normalized;
        }
    }
}
