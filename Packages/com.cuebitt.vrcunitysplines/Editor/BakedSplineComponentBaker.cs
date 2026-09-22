using UnityEditor;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Copies baked arrays from the in-memory bake onto Udon behaviours.
    // This is the bridge: Udon reads the component fields, nothing else.
    public static class BakedSplineComponentBaker
    {
        public static void Fill(BakedSpline data, VRCSplineTweenDriver driver)
        {
            if (data == null || driver == null) return;

            // the tween driver only needs positions and the closed flag
            driver.positions = data.positions;
            driver.closed = data.closed;
            EditorUtility.SetDirty(driver);
        }

        public static void Fill(BakedSpline data, VRCSplineEvaluator evaluator)
        {
            if (data == null || evaluator == null) return;

            // the evaluator needs every channel for distance queries
            evaluator.positions = data.positions;
            evaluator.tangents = data.tangents;
            evaluator.upVectors = data.upVectors;
            evaluator.cumulativeLengths = data.cumulativeLengths;
            evaluator.totalLength = data.totalLength;
            evaluator.closed = data.closed;
            EditorUtility.SetDirty(evaluator);
        }
    }
}
