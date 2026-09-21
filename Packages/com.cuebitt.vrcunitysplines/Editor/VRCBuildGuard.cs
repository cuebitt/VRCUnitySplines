using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // VRChat whitelists neither UnityEngine.Splines nor Udon-unfriendly math,
    // so any live Splines component left in the scene fails the upload.
    // This guard stops the build early with names instead of a cryptic error.
    public class VRCBuildGuard : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 1000;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            // sweep the scene for anything Splines-flavored
            var containers = Object.FindObjectsOfType<SplineContainer>(true);
            var animates = Object.FindObjectsOfType<SplineAnimate>(true);
            var instances = Object.FindObjectsOfType<SplineInstantiate>(true);
            var extrudes = Object.FindObjectsOfType<SplineExtrude>(true);

            // clean scene, let the build through
            int total = containers.Length + animates.Length + instances.Length + extrudes.Length;
            if (total == 0) return true;

            // name one so the user knows where to look
            string first = containers.Length > 0 ? containers[0].name
                : animates.Length > 0 ? animates[0].name
                : instances.Length > 0 ? instances[0].name : extrudes[0].name;

            EditorUtility.DisplayDialog("VRCUnitySplines: bake first",
                "Found " + total + " live Splines component(s) (e.g. '" + first + "').\n\n" +
                "Bake them with VRCUnitySplines > Bake Window, then delete the " +
                "SplineContainer / SplineAnimate / SplineInstantiate / SplineExtrude " +
                "components before uploading. Only baked assets are upload-safe.",
                "OK");
            return false;
        }
    }
}
