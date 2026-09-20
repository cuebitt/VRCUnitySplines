using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Cuebitt.VRCUnitySplines.Editor
{
    public enum BakedLoopMode { Once, Loop, PingPong }

    // Default animate output: resampled baked frames become position and
    // rotation keys with linear tangents. Frames are arc-length uniform, so
    // uniform key timing gives constant speed with zero runtime Udon.
    public static class AnimateClipBaker
    {
        public static AnimationClip BakeClip(VRCBakedSplineData data, float durationSeconds, BakedLoopMode loop)
        {
            int frames = data.positions.Length;
            var clip = new AnimationClip { frameRate = 60f };
            clip.wrapMode = loop switch
            {
                BakedLoopMode.Loop => WrapMode.Loop,
                BakedLoopMode.PingPong => WrapMode.PingPong,
                _ => WrapMode.ClampForever,
            };

            var posX = new Keyframe[frames];
            var posY = new Keyframe[frames];
            var posZ = new Keyframe[frames];
            var rotX = new Keyframe[frames];
            var rotY = new Keyframe[frames];
            var rotZ = new Keyframe[frames];
            var rotW = new Keyframe[frames];

            for (int i = 0; i < frames; i++)
            {
                float time = durationSeconds * i / (frames - 1);
                Vector3 p = data.positions[i];
                Quaternion q = Quaternion.LookRotation(data.tangents[i], data.upVectors[i]);
                posX[i] = new Keyframe(time, p.x);
                posY[i] = new Keyframe(time, p.y);
                posZ[i] = new Keyframe(time, p.z);
                rotX[i] = new Keyframe(time, q.x);
                rotY[i] = new Keyframe(time, q.y);
                rotZ[i] = new Keyframe(time, q.z);
                rotW[i] = new Keyframe(time, q.w);
            }

            SetLinearTangents(posX); SetLinearTangents(posY); SetLinearTangents(posZ);
            SetLinearTangents(rotX); SetLinearTangents(rotY); SetLinearTangents(rotZ); SetLinearTangents(rotW);

            clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(posX));
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", new AnimationCurve(posY));
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.z", new AnimationCurve(posZ));
            clip.SetCurve("", typeof(Transform), "m_LocalRotation.x", new AnimationCurve(rotX));
            clip.SetCurve("", typeof(Transform), "m_LocalRotation.y", new AnimationCurve(rotY));
            clip.SetCurve("", typeof(Transform), "m_LocalRotation.z", new AnimationCurve(rotZ));
            clip.SetCurve("", typeof(Transform), "m_LocalRotation.w", new AnimationCurve(rotW));

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop != BakedLoopMode.Once;
            settings.loopBlend = loop == BakedLoopMode.Loop && data.closed;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        public static Animator WireAnimator(GameObject target, AnimationClip clip, float startOffset01)
        {
            string dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip));
            string controllerPath = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + target.name + "_Spline.controller");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var state = controller.layers[0].stateMachine.AddState(clip.name);
            state.motion = clip;
            state.speed = 1f;

            var animator = target.GetComponent<Animator>();
            if (animator == null) animator = target.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            if (startOffset01 > 0f) animator.Play(0, 0, startOffset01 % 1f);
            return animator;
        }

        private static void SetLinearTangents(Keyframe[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                int prev = Mathf.Max(0, i - 1);
                int next = Mathf.Min(keys.Length - 1, i + 1);
                float dt = keys[next].time - keys[prev].time;
                float slope = dt > 1e-9f ? (keys[next].value - keys[prev].value) / dt : 0f;
                keys[i].inTangent = slope;
                keys[i].outTangent = slope;
            }
        }
    }
}
