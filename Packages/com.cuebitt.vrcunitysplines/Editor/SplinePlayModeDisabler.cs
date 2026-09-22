using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Turns live Splines components off while the editor plays so baked
    // outputs are not fought over by the real splines, then puts them back
    // on exit. State goes through SessionState because entering play does a
    // domain reload and would wipe ordinary statics.
    [InitializeOnLoad]
    internal static class SplinePlayModeDisabler
    {
        private const string DisabledKey = "VRCUnitySplines.DisabledBehaviours";

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

            foreach (var behaviour in FindSplinesBehaviours())
            {
                if (!behaviour.enabled) continue;
                behaviour.enabled = false;
                disabled.Add(behaviour.GetInstanceID().ToString());
            }

            SessionState.SetString(DisabledKey, string.Join(",", disabled));
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

            SessionState.SetString(DisabledKey, "");
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
