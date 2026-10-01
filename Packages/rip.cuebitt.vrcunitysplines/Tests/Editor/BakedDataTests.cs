using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Cuebitt.VRCUnitySplines.Editor;

namespace Cuebitt.VRCUnitySplines.Tests
{
    // These run with no Splines package present on purpose: everything under
    // test consumes already-baked data, which is the upload-safe half.
    public class BakedDataTests
    {
        private static EditorCurveBinding FloatBinding(string property) =>
            EditorCurveBinding.FloatCurve("", typeof(Transform), property);
        private static BakedSpline StraightLine(int frames = 11)
        {
            // dead simple line along x, one unit per frame
            var data = new BakedSpline();
            data.positions = new Vector3[frames];
            data.tangents = new Vector3[frames];
            data.upVectors = new Vector3[frames];
            data.cumulativeLengths = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                data.positions[i] = new Vector3(i, 0f, 0f);
                data.tangents[i] = Vector3.right;
                data.upVectors[i] = Vector3.up;
                data.cumulativeLengths[i] = i;
            }
            data.totalLength = frames - 1;
            return data;
        }

        [Test]
        public void LengthTable_IsMonotonicAndTotalsLength()
        {
            // lengths should climb steadily and sum to the full line
            var data = StraightLine();
            Assert.AreEqual(10f, data.totalLength, 1e-5f);
            for (int i = 1; i < data.cumulativeLengths.Length; i++)
                Assert.Greater(data.cumulativeLengths[i], data.cumulativeLengths[i - 1]);
        }

        [Test]
        public void ClipBaker_EmitsPositionAndRotationCurvesAtDuration()
        {
            // one key per frame, clip as long as asked, looping as told
            var data = StraightLine();
            var clip = AnimateClipBaker.BakeClip(data, 4f, BakedLoopMode.Loop);
            Assert.AreEqual(4f, clip.length, 1e-3f);
            Assert.AreEqual(WrapMode.Loop, clip.wrapMode);
            var posX = AnimationUtility.GetEditorCurve(clip, FloatBinding("m_LocalPosition.x"));
            var rotW = AnimationUtility.GetEditorCurve(clip, FloatBinding("m_LocalRotation.w"));
            Assert.IsNotNull(posX);
            Assert.IsNotNull(rotW);
            Assert.AreEqual(11, posX.length);
        }

        [Test]
        public void Evaluator_GetPositionAt_SamplesBakedLine()
        {
            // midpoint of the 10-unit line sits at x=5
            var data = StraightLine();
            var go = new GameObject("EvaluatorTest");
            var evaluator = go.AddComponent<Cuebitt.VRCUnitySplines.VRCSplineEvaluator>();
            evaluator.positions = data.positions;
            evaluator.tangents = data.tangents;
            evaluator.upVectors = data.upVectors;
            evaluator.cumulativeLengths = data.cumulativeLengths;
            evaluator.totalLength = data.totalLength;
            evaluator.closed = data.closed;

            Assert.AreEqual(new Vector3(5f, 0f, 0f), evaluator.GetPositionAt(0.5f));

            Object.DestroyImmediate(go);
        }
    }
}
