using UnityEditor;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Copies baked data from the editor-only asset onto Udon behaviours as
    // plain arrays. This is the bridge: Udon reads the component fields, the
    // ScriptableObject never leaves the editor.
    public static class BakedSplineComponentBaker
    {
        public static void Fill(VRCBakedSplineData data, VRCSplineTweenDriver driver)
        {
            if (data == null || driver == null) return;

            // the tween driver only needs positions and the closed flag
            driver.positions = (Vector3[])data.positions.Clone();
            driver.closed = data.closed;
            EditorUtility.SetDirty(driver);
        }

        public static void Fill(VRCBakedSplineData data, VRCSplineEvaluator evaluator)
        {
            if (data == null || evaluator == null) return;

            // the evaluator needs every channel for distance queries
            evaluator.positions = (Vector3[])data.positions.Clone();
            evaluator.tangents = (Vector3[])data.tangents.Clone();
            evaluator.upVectors = (Vector3[])data.upVectors.Clone();
            evaluator.cumulativeLengths = (float[])data.cumulativeLengths.Clone();
            evaluator.totalLength = data.totalLength;
            evaluator.closed = data.closed;
            EditorUtility.SetDirty(evaluator);
        }
    }
}
