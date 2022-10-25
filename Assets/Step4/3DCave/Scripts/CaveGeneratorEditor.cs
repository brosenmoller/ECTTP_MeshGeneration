using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CaveGenerator), true)]
public class CaveGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CaveGenerator ca = (CaveGenerator)target;

        if (DrawDefaultInspector())
        {
            if (ca.autoUpdate)
            {
                ca.GenerateCave();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            ca.GenerateCave();
        }
    }
}
