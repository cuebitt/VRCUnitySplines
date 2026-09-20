using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Converts one SplineContainer spline into a VRCBakedSplineData asset.
    // Only uses SplineContainer.Evaluate*, so it works across Splines 2.x.
    public static class SplineBaker
    {
        public static VRCBakedSplineData Bake(SplineContainer container, int splineIndex, int samplesPerCurve = 32)
        {
            var spline = container.Splines[splineIndex];
            int curves = Mathf.Max(1, spline.Count - (spline.Closed ? 0 : 1));
            int frames = Mathf.Max(2, curves * samplesPerCurve + 1);

            var data = ScriptableObject.CreateInstance<VRCBakedSplineData>();
            data.positions = new Vector3[frames];
            data.tangents = new Vector3[frames];
            data.upVectors = new Vector3[frames];
            data.cumulativeLengths = new float[frames];

            Transform root = container.transform;
            Vector3 prev = Vector3.zero;
            float length = 0f;

            for (int i = 0; i < frames; i++)
            {
                float t = frames == 1 ? 0f : (float)i / (frames - 1);
                container.Evaluate(splineIndex, t, out Vector3 worldPos, out Vector3 worldTan, out Vector3 worldUp);

                Vector3 pos = root.InverseTransformPoint(worldPos);
                Vector3 tan = root.InverseTransformDirection(worldTan);
                Vector3 up = root.InverseTransformDirection(worldUp);

                if (tan.sqrMagnitude < 1e-10f) tan = i > 0 ? data.tangents[i - 1] : Vector3.forward;
                if (up.sqrMagnitude < 1e-10f || Vector3.Cross(tan, up).sqrMagnitude < 1e-10f) up = Vector3.up;

                data.positions[i] = pos;
                data.tangents[i] = tan.normalized;
                data.upVectors[i] = up.normalized;

                if (i > 0) length += Vector3.Distance(pos, prev);
                data.cumulativeLengths[i] = length;
                prev = pos;
            }

            data.totalLength = length;
            data.closed = spline.Closed;
            data.sourceDescription = container.name + " [" + splineIndex + "] @" + samplesPerCurve + "/curve";

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Baked Spline", container.name + "_spline" + splineIndex, "asset",
                "Where to store the baked spline data.");
            if (string.IsNullOrEmpty(path))
                return null;

            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<VRCBakedSplineData>(path);
        }

        public static VRCBakedSplineData[] BakeAll(SplineContainer container, int samplesPerCurve = 32)
        {
            var baked = new VRCBakedSplineData[container.Splines.Count];
            for (int i = 0; i < baked.Length; i++)
            {
                baked[i] = Bake(container, i, samplesPerCurve);
                if (baked[i] == null) return null;
            }
            return baked;
        }
    }
}
