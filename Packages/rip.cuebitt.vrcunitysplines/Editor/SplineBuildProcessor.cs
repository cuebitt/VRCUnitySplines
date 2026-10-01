using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Strips live Splines components while scenes are being built, so
    // uploads never contain them while the editable scene keeps them for
    // further editing. OnProcessScene only sees the build's own scene
    // instance, never the open editor scene, so nothing here needs undoing.
    // Matches by namespace so every current and future Splines component
    // is covered without tracking type lists.
    public class SplineBuildProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var components = root.GetComponentsInChildren<Component>(true);
                foreach (var component in components)
                {
                    // null covers missing scripts, namespace matches Splines
                    if (component == null) continue;
                    if (component.GetType().Namespace != "UnityEngine.Splines") continue;

                    // immediate, the scene gets serialized right after this
                    Object.DestroyImmediate(component);
                }
            }
        }
    }
}
