using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public enum MapType { PerlinNoise, CellularAutomata }

    [Header("General")]
    public bool autoUpdate;
    [SerializeField] MapType mapType;
    [SerializeField] int mapSize;
    [SerializeField] int seed;

    [Header("Noise")]
    [SerializeField][Range(0, 1)] float perlinCutoff;
    [SerializeField] float scale;

    [Header("CelluarAutomata")]
    [SerializeField][Range(0, 100)] int randomFillPercent;
    [SerializeField][Range(0, 10)] int smoothInterations;
    [SerializeField][Range(0, 36)] int wallCuttoffUpper;
    [SerializeField][Range(0, 36)] int wallCuttoffLower;

    [Header("Mesh")]
    [SerializeField] bool generateMesh;
    [SerializeField] int surfaceLevel;
    [SerializeField] float squareSize;


    int[,,] map;

    public void GenerateCave()
    {
        switch (mapType)
        {
            case MapType.PerlinNoise: map = Noise3D.GenerateMap(mapSize, scale, seed); break;
            case MapType.CellularAutomata: map = CellularAutomata3D.GenerateMap(mapSize, seed, smoothInterations, randomFillPercent, wallCuttoffLower, wallCuttoffUpper); break;
        }

        GetComponent<MarchingCubesMesh>().GenerateMesh(map, squareSize, surfaceLevel);
    }

    private void OnDrawGizmos()
    {
        if (map == null || generateMesh) return;

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    Gizmos.color = Color.red;
                    if (map[x, y, z] == 1) Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                }
            }
        }
    }

    private void OnValidate()
    {
        if (wallCuttoffUpper < wallCuttoffLower) wallCuttoffLower = wallCuttoffUpper;
    }
}
