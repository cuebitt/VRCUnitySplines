using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Turns live Splines components off while the editor plays so baked
    // outputs are not fought over by the real splines, then puts them back
    // on exit. State goes through SessionState because entering play does a
    // domain reload and would wipe ordinary statics. Scenes that were clean
    // before the toggle get cleaned up after it, otherwise every play
    // session would nag about unsaved changes we caused ourselves.
    [InitializeOnLoad]
    internal static class SplinePlayModeDisabler
    {
        private const string DisabledKey = "VRCUnitySplines.DisabledBehaviours";
        private const string CleanScenesKey = "VRCUnitySplines.CleanScenes";

        static SplinePlayModeDisabler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) Disable();
            else if (state == PlayModeStateChange.EnteredEditMode) Restore();
        }

        private static void Disable()
        {
            var disabled = new List<string>();
            var cleanScenes = new List<string>();

            foreach (var behaviour in FindSplinesBehaviours())
            {
                // note the scene as clean before the toggle dirties it
                var scene = behaviour.gameObject.scene;
                if (scene.IsValid() && scene.path.Length > 0 && !scene.isDirty
                    && !cleanScenes.Contains(scene.path))
                    cleanScenes.Add(scene.path);

                if (!behaviour.enabled) continue;
                behaviour.enabled = false;
                disabled.Add(behaviour.GetInstanceID().ToString());
            }

            SessionState.SetString(DisabledKey, string.Join(",", disabled));
            SessionState.SetString(CleanScenesKey, string.Join("\n", cleanScenes));
        }

        private static void Restore()
        {
            // only re-enable what we turned off ourselves
            var disabled = SessionState.GetString(DisabledKey, "");
            if (disabled.Length > 0)
            {
                var wanted = new HashSet<string>(disabled.Split(','));
                foreach (var behaviour in FindSplinesBehaviours())
                    if (wanted.Contains(behaviour.GetInstanceID().ToString()))
                        behaviour.enabled = true;
            }

            // clear the dirty flag we added, only on scenes we recorded clean
            var cleanScenes = SessionState.GetString(CleanScenesKey, "");
            foreach (var path in cleanScenes.Split('\n'))
            {
                if (path.Length == 0) continue;
                var scene = SceneManager.GetSceneByPath(path);
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.MarkSceneClean(scene);
            }

            SessionState.SetString(DisabledKey, "");
            SessionState.SetString(CleanScenesKey, "");
        }

        private static Behaviour[] FindSplinesBehaviours()
        {
            // SplineComponent covers Animate and Instantiate, the other two stand alone
            var found = new List<Behaviour>();
            found.AddRange(Object.FindObjectsOfType<SplineContainer>(true));
            found.AddRange(Object.FindObjectsOfType<SplineComponent>(true));
            found.AddRange(Object.FindObjectsOfType<SplineExtrude>(true));
            return found.ToArray();
        }
    }
}
