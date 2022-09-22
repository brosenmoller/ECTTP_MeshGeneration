using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshGenerator meshGenerator = (MeshGenerator)target;

        if (DrawDefaultInspector())
        {
            if (meshGenerator.autoUpdate)
            {
                meshGenerator.DrawMesh();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            meshGenerator.DrawMesh();
        }

        if (GUILayout.Button("Scroll"))
        {
            meshGenerator.Scroll();
        }
    }
}
