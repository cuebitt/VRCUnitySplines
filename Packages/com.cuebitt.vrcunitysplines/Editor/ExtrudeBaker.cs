using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuebitt.VRCUnitySplines.Editor
{
    // Persists Unity's SplineExtrude output to a Mesh asset so the result is
    // a plain MeshFilter/MeshRenderer pair. Point it at the SplineExtrude,
    // bake, then delete the Splines components before upload.
    public static class ExtrudeBaker
    {
        public static Mesh BakeMesh(SplineExtrude extrude)
        {
            extrude.Rebuild();

            var filter = extrude.GetComponent<MeshFilter>();
            Mesh source = filter != null ? filter.sharedMesh : null;
            if (source == null)
            {
                Debug.LogError("VRCUnitySplines: SplineExtrude produced no mesh to bake.");
                return null;
            }

            var copy = Object.Instantiate(source);
            copy.name = extrude.gameObject.name + "_BakedMesh";

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Baked Extrude Mesh", copy.name, "asset",
                "Where to store the baked extrude mesh.");
            if (string.IsNullOrEmpty(path))
                return null;

            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();

            extrude.targetMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            EditorUtility.SetDirty(extrude);
            return extrude.targetMesh;
        }
    }
}
