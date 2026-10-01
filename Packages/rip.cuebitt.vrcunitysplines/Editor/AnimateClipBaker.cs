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
        public static AnimationClip BakeClip(BakedSpline data, float durationSeconds, BakedLoopMode loop)
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

            string[] props =
            {
                "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z",
                "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w"
            };
            Keyframe[][] channelKeys = { posX, posY, posZ, rotX, rotY, rotZ, rotW };
            for (int c = 0; c < props.Length; c++)
            {
                var curve = new AnimationCurve(channelKeys[c]);

                for (int i = 0; i < curve.length; i++)
                {
                    AnimationUtility.SetKeyBroken(curve, i, true);
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }

                clip.SetCurve("", typeof(Transform), props[c], curve);
            }

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop != BakedLoopMode.Once;
            settings.loopBlend = loop == BakedLoopMode.Loop && data.closed;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        public static Animator WireAnimator(GameObject target, AnimationClip clip)
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
            return animator;
        }
    }
}
