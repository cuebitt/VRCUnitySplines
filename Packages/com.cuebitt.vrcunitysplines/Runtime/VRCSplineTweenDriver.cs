using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace Cuebitt.VRCUnitySplines
{
    // Fallback animate output. Drives the object along baked waypoints with
    // native DOTween timing, so no per-frame Udon math runs. Position only:
    // use the AnimationClip baker when baked orientation matters.
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRCSplineTweenDriver : UdonSharpBehaviour
    {
        [Tooltip("Baked spline to follow. Waypoints come from its positions.")]
        public VRCBakedSplineData spline;

        [Tooltip("Take every Nth baked frame as a waypoint. Higher is cheaper.")]
        [Min(1)] public int waypointStride = 4;

        [Tooltip("Seconds per traversal.")]
        [Min(0.01f)] public float duration = 5f;

        [Tooltip("Restart seamlessly at the end instead of stopping.")]
        public bool loop = true;

        [Tooltip("Start moving in Start().")]
        public bool playOnAwake = true;

        private VRCTweenHandle _tween;

        void Start()
        {
            if (playOnAwake) Play();
        }

        public void Play()
        {
            // nothing to follow, bail out quietly
            if (spline == null || spline.positions == null || spline.positions.Length < 2) return;
            Stop();

            // thin the baked frames down to a waypoint list
            int frames = spline.positions.Length;
            int count = Mathf.Max(2, Mathf.CeilToInt((float)frames / Mathf.Max(1, waypointStride)));
            var waypoints = new Vector3[count];
            for (int i = 0; i < count; i++)
                waypoints[i] = spline.positions[Mathf.Min(frames - 1, i * waypointStride)];

            // hand it to DOTween, native side takes it from here
            _tween = gameObject.TweenLocalPath(
                waypoints, Mathf.Max(0.01f, duration),
                VRCTweenPathType.CatmullRom, spline.closed, 10, VRCTweenEase.Linear);
            if (loop) _tween.SetLoops(-1, VRCTweenLoopType.Restart);
        }

        public void Stop()
        {
            // only kill if there is something running
            if (_tween.IsValid) _tween.Kill();
        }

        void OnDestroy()
        {
            // never leave a tween behind on a dead object
            gameObject.KillAllTweens();
        }
    }
}
