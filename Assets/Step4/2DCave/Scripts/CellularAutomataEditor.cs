using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CellularAutomata), true)]
public class CellularAutomataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CellularAutomata ca = (CellularAutomata)target;

        if (DrawDefaultInspector())
        {
            if (ca.autoUpdate)
            {
                ca.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            ca.GenerateMap();
        }
    }
}
