using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuebitt.VRCUnitySplines.Editor
{
    public class SplineBakeWindow : EditorWindow
    {
        private SplineContainer _container;
        private int _splineIndex;
        private int _samplesPerCurve = 32;

        // serialized so the bake survives domain reloads
        [SerializeField] private BakedSpline _data;
        private GameObject _target;
        private float _duration = 6f;
        private BakedLoopMode _loop = BakedLoopMode.Loop;
        private VRCSplineTweenDriver _tweenDriver;
        private VRCSplineEvaluator _evaluator;

        [MenuItem("Tools/VRCUnitySplines/Bake Spline")]
        public static void Open() => GetWindow<SplineBakeWindow>("Spline Bake");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("1. Bake spline data", EditorStyles.boldLabel);
            _container = (SplineContainer)EditorGUILayout.ObjectField("Container", _container, typeof(SplineContainer), true);
            _splineIndex = EditorGUILayout.IntField("Spline index", _splineIndex);
            _samplesPerCurve = EditorGUILayout.IntSlider("Samples per curve", _samplesPerCurve, 4, 128);
            if (GUILayout.Button("Bake Data") && _container != null)
                _data = SplineBaker.Bake(_container, _splineIndex, _samplesPerCurve);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("2. Outputs", EditorStyles.boldLabel);
            if (_data == null)
            {
                EditorGUILayout.HelpBox("No bake yet. Press Bake Data above.", MessageType.Info);
                return;
            }
            EditorGUILayout.LabelField("Baked", _data.sourceDescription + ", " + _data.positions.Length + " frames, " + _data.totalLength.ToString("F2") + " units");

            _target = (GameObject)EditorGUILayout.ObjectField("Animate target", _target, typeof(GameObject), true);
            _duration = EditorGUILayout.FloatField("Duration (s)", Mathf.Max(0.1f, _duration));
            _loop = (BakedLoopMode)EditorGUILayout.EnumPopup("Loop", _loop);
            if (GUILayout.Button("Bake AnimationClip (default)") && _target != null)
                BakeClip();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("3. Baked components (Udon data)", EditorStyles.boldLabel);
            _tweenDriver = (VRCSplineTweenDriver)EditorGUILayout.ObjectField("Tween driver", _tweenDriver, typeof(VRCSplineTweenDriver), true);
            if (GUILayout.Button("Fill Tween Driver") && _data != null && _tweenDriver != null)
            {
                _tweenDriver.positions = _data.positions;
                _tweenDriver.closed = _data.closed;
                EditorUtility.SetDirty(_tweenDriver);
            }
            _evaluator = (VRCSplineEvaluator)EditorGUILayout.ObjectField("Evaluator", _evaluator, typeof(VRCSplineEvaluator), true);
            if (GUILayout.Button("Fill Evaluator") && _data != null && _evaluator != null)
            {
                _evaluator.positions = _data.positions;
                _evaluator.tangents = _data.tangents;
                _evaluator.upVectors = _data.upVectors;
                _evaluator.cumulativeLengths = _data.cumulativeLengths;
                _evaluator.totalLength = _data.totalLength;
                _evaluator.closed = _data.closed;
                EditorUtility.SetDirty(_evaluator);
            }
        }

        private void BakeClip()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save AnimationClip", _target.name + "_SplineAnim", "anim",
                "Where to store the baked animation clip.");
            if (string.IsNullOrEmpty(path)) return;

            var clip = AnimateClipBaker.BakeClip(_data, _duration, _loop);
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            AnimateClipBaker.WireAnimator(_target, clip);
        }

        // Builds a small S-curve bake from scratch so new users can try the
        // outputs without installing anything beyond this package.
        [MenuItem("Tools/VRCUnitySplines/Create Demo Spline")]
        public static void CreateDemo()
        {
            const int frames = 65;
            var data = new BakedSpline();
            data.positions = new Vector3[frames];
            data.tangents = new Vector3[frames];
            data.upVectors = new Vector3[frames];
            data.cumulativeLengths = new float[frames];

            float length = 0f;
            Vector3 prev = Vector3.zero;
            for (int i = 0; i < frames; i++)
            {
                float u = (float)i / (frames - 1);
                Vector3 p = new Vector3(Mathf.Lerp(-4f, 4f, u), 0f, Mathf.Sin(u * Mathf.PI * 2f));
                data.positions[i] = p;
                data.upVectors[i] = Vector3.up;
                if (i > 0) length += Vector3.Distance(p, prev);
                data.cumulativeLengths[i] = length;
                prev = p;
            }

            for (int i = 0; i < frames; i++)
            {
                Vector3 a = data.positions[Mathf.Max(0, i - 1)];
                Vector3 b = data.positions[Mathf.Min(frames - 1, i + 1)];
                data.tangents[i] = (b - a).normalized;
            }
            data.totalLength = length;
            data.closed = false;
            data.sourceDescription = "Built-in demo S-curve";

            if (!AssetDatabase.IsValidFolder("Assets/VRCUnitySplinesDemo"))
                AssetDatabase.CreateFolder("Assets", "VRCUnitySplinesDemo");

            var demo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            demo.name = "DemoSplineFollower";
            var clip = AnimateClipBaker.BakeClip(data, 6f, BakedLoopMode.PingPong);
            AssetDatabase.CreateAsset(clip, "Assets/VRCUnitySplinesDemo/DemoSplineAnim.anim");
            AssetDatabase.SaveAssets();
            AnimateClipBaker.WireAnimator(demo, clip);
            Selection.activeGameObject = demo;
        }
    }
}
