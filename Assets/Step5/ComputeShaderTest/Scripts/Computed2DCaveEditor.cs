using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Computed2DCave), true)]
public class Computed2DCaveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Computed2DCave cc = (Computed2DCave)target;

        if (DrawDefaultInspector())
        {
            if (cc.autoUpdate)
            {
                cc.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            cc.GenerateMap();
        }
    }
}
