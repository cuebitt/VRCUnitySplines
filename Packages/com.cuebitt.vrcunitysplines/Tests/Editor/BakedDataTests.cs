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
        private static VRCBakedSplineData StraightLine(int frames = 11)
        {
            var data = ScriptableObject.CreateInstance<VRCBakedSplineData>();
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
            var data = StraightLine();
            Assert.AreEqual(10f, data.totalLength, 1e-5f);
            for (int i = 1; i < data.cumulativeLengths.Length; i++)
                Assert.Greater(data.cumulativeLengths[i], data.cumulativeLengths[i - 1]);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void ClipBaker_EmitsPositionAndRotationCurvesAtDuration()
        {
            var data = StraightLine();
            var clip = AnimateClipBaker.BakeClip(data, 4f, BakedLoopMode.Loop);
            Assert.AreEqual(4f, clip.length, 1e-3f);
            Assert.AreEqual(WrapMode.Loop, clip.wrapMode);
            var posX = AnimationUtility.GetEditorCurve(clip, FloatBinding("m_LocalPosition.x"));
            var rotW = AnimationUtility.GetEditorCurve(clip, FloatBinding("m_LocalRotation.w"));
            Assert.IsNotNull(posX);
            Assert.IsNotNull(rotW);
            Assert.AreEqual(11, posX.length);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void InstantiateBaker_PlacesRequestedCount()
        {
            var data = StraightLine();
            var source = new GameObject("BakeTestSource");
            string prefabPath = "Assets/BakeTestPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var holder = new GameObject("BakeTestHolder");

            var group = InstantiateBaker.BakeByCount(data, holder.transform, prefab, 5,
                Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, 1f, 1f, seed: 7);

            Assert.AreEqual(5, group.transform.childCount);
            Assert.AreEqual(new Vector3(0f, 0f, 0f), group.transform.GetChild(0).localPosition);
            Assert.AreEqual(new Vector3(10f, 0f, 0f), group.transform.GetChild(4).localPosition);

            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(source);
            AssetDatabase.DeleteAsset(prefabPath);
            Object.DestroyImmediate(data);
        }
    }
}
