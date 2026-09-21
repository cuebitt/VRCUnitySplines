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
            // make sure the live mesh is up to date first
            extrude.Rebuild();

            // grab whatever it generated
            var filter = extrude.GetComponent<MeshFilter>();
            Mesh source = filter != null ? filter.sharedMesh : null;
            if (source == null)
            {
                Debug.LogError("VRCUnitySplines: SplineExtrude produced no mesh to bake.");
                return null;
            }

            // duplicate so the asset owns its own copy
            var copy = Object.Instantiate(source);
            copy.name = extrude.gameObject.name + "_BakedMesh";

            // let the user pick the save spot, cancelled means no asset
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Baked Extrude Mesh", copy.name, "asset",
                "Where to store the baked extrude mesh.");
            if (string.IsNullOrEmpty(path))
                return null;

            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();

            // point the filter at the persisted mesh
            Mesh baked = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            filter.sharedMesh = baked;
            EditorUtility.SetDirty(filter);
            return baked;
        }
    }
}
