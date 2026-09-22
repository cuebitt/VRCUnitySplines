using UnityEditor;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Editor-time prefab scatter along baked frames. Output is plain
    // GameObjects, so there is no runtime cost and nothing upload-unsafe.
    public static class InstantiateBaker
    {
        public static GameObject BakeByCount(BakedSpline data, Transform parent, GameObject prefab,
            int count, Vector3 minOffset, Vector3 maxOffset, Vector3 minEuler, Vector3 maxEuler,
            float minScale, float maxScale, int seed, bool alignToTangent = true)
        {
            // even spread including both ends, then reuse the spacing path
            if (count <= 0) return null;
            float spacing = count == 1 ? 0f : data.totalLength / (count - 1);
            return BakeBySpacing(data, parent, prefab, spacing, count,
                minOffset, maxOffset, minEuler, maxEuler, minScale, maxScale, seed, alignToTangent);
        }

        public static GameObject BakeBySpacing(BakedSpline data, Transform parent, GameObject prefab,
            float spacing, int maxCount, Vector3 minOffset, Vector3 maxOffset, Vector3 minEuler, Vector3 maxEuler,
            float minScale, float maxScale, int seed, bool alignToTangent = true)
        {
            // everything lands in one group so it can be cleared later
            var group = new GameObject("VRCBaked " + data.sourceDescription);
            Undo.RegisterCreatedObjectUndo(group, "Bake spline instances");
            group.transform.SetParent(parent, false);

            // seeded rng keeps the scatter deterministic
            var rng = new System.Random(seed);
            float distance = 0f;
            int placed = 0;

            // march along the spline dropping prefabs
            while (distance <= data.totalLength + 1e-4f && placed < Mathf.Max(1, maxCount))
            {
                Sample(data, distance, out Vector3 pos, out Quaternion rot);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
                Undo.RegisterCreatedObjectUndo(instance, "Bake spline instances");

                // roll the random offsets for this one
                Vector3 offset = new Vector3(
                    Lerp(minOffset.x, maxOffset.x, rng),
                    Lerp(minOffset.y, maxOffset.y, rng),
                    Lerp(minOffset.z, maxOffset.z, rng));
                Vector3 euler = new Vector3(
                    Lerp(minEuler.x, maxEuler.x, rng),
                    Lerp(minEuler.y, maxEuler.y, rng),
                    Lerp(minEuler.z, maxEuler.z, rng));
                float scale = Mathf.Lerp(minScale, maxScale, (float)rng.NextDouble());

                // pose it, tangent facing plus the random rotation on top
                instance.transform.localPosition = pos + offset;
                instance.transform.localRotation = (alignToTangent ? rot : Quaternion.identity) * Quaternion.Euler(euler);
                instance.transform.localScale = Vector3.one * Mathf.Max(1e-4f, scale);

                placed++;
                distance += Mathf.Max(1e-4f, spacing);
                if (spacing <= 1e-4f) break;
            }
            return group;
        }

        public static void ClearBaked(Transform parent)
        {
            // only touches groups this baker made, nothing else
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child.name.StartsWith("VRCBaked "))
                    Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        private static void Sample(BakedSpline data, float distance, out Vector3 pos, out Quaternion rot)
        {
            // find the bracketing frames in the length table
            float[] lengths = data.cumulativeLengths;
            int frame = lengths.Length - 1;
            for (int i = 1; i < lengths.Length; i++)
                if (lengths[i] >= distance) { frame = i; break; }

            // blend position and direction, then build the facing
            float span = lengths[frame] - lengths[frame - 1];
            float f = span > 1e-9f ? (distance - lengths[frame - 1]) / span : 0f;
            pos = Vector3.Lerp(data.positions[frame - 1], data.positions[frame], f);
            Vector3 tan = Vector3.Lerp(data.tangents[frame - 1], data.tangents[frame], f);
            Vector3 up = Vector3.Lerp(data.upVectors[frame - 1], data.upVectors[frame], f);
            rot = tan.sqrMagnitude > 1e-8f
                ? Quaternion.LookRotation(tan.normalized, up.sqrMagnitude > 1e-8f ? up : Vector3.up)
                : Quaternion.identity;
        }

        private static float Lerp(float a, float b, System.Random rng) =>
            a + (b - a) * (float)rng.NextDouble();
    }
}
