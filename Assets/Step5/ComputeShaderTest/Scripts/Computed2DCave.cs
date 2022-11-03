using UnityEngine;

// Tutorial by Sebastian Lague: https://www.youtube.com/watch?v=v7yyZZjF1z4&t=1166s
public class Computed2DCave : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] int mapSize;
    [SerializeField] int seed;

    [Header("Walls")]
    [SerializeField][Range(0, 100)] int randomFillPercent;
    [SerializeField][Range(0, 10)] int smoothInterations;
    [SerializeField][Range(0, 9)] int wallCuttoff;
    [SerializeField] float wallHeight;

    [Header("Mesh Generation")]
    [SerializeField] float squareSize = 1;
    [SerializeField] bool generateMesh;

    [Header("Computer")]
    [SerializeField] ComputeShader computeShader;

    int[] map;

    void Start()
    {
        GenerateMap();
    }

    [ContextMenu("Generate")]
    public void GenerateMap()
    {
        map = new int[mapSize * mapSize];
        RandomFillMap();

        ComputeBuffer mapBuffer = new(map.Length, sizeof(int));
        mapBuffer.SetData(map);

        computeShader.SetBuffer(0, "map", mapBuffer);
        computeShader.SetInt("wallCutoff", wallCuttoff);
        computeShader.SetInt("mapSize", mapSize);

        for (int i = 0; i < smoothInterations; i++)
        {
            computeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
        }

        mapBuffer.GetData(map);
        mapBuffer.Dispose();

        if (generateMesh)
        {
            MarchingSquaresCompute meshGen = GetComponent<MarchingSquaresCompute>();
            meshGen.GenerateMesh(map, squareSize, wallHeight, mapSize);
        }
    }

    void RandomFillMap()
    {
        System.Random rand = new(seed);

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (x == 0 || x == mapSize - 1 || y == 0 || y == mapSize - 1)
                {
                    map[x + y * mapSize] = 1;
                }
                else
                {
                    map[x + y * mapSize] = rand.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (map != null && !generateMesh)
        {
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    Gizmos.color = map[x + y * mapSize] == 1 ? Color.black : Color.green;
                    Vector3 position = new(-mapSize / 2 + x + .5f, 0, -mapSize / 2 + y + .5f);
                    Gizmos.DrawCube(position, Vector3.one);
                }
            }
        }
    }
}
