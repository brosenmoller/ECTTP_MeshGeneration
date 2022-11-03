using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComputedTerrainGenerator))]
public class ComputedTerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ComputedTerrainGenerator terrainGenerator = (ComputedTerrainGenerator)target;

        if (DrawDefaultInspector())
        {
            if (terrainGenerator.autoUpdate)
            {
                terrainGenerator.GenerateTerrain();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            terrainGenerator.GenerateTerrain();
        }
    }
}

