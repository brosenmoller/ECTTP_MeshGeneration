using UnityEngine;

public class ComputedTerrainGenerator : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] int mapSize;
    [SerializeField] int seed;

    [Header("Noise")]
    [SerializeField][Range(0, 1)] float surfaceLevel;
    [SerializeField] float scale;
    [Range(1, 10)] public int octaves;
    [Range(0, 1f)] public float persistence;
    [Range(1, 10)] public float lacunarity;
    [Range(0, 1f)] public float weightMultiplier;

    [Header("Mesh")]
    [SerializeField] bool generateMesh;
    [SerializeField] float squareSize;
    [SerializeField] ComputedMarchingCubesMesh meshGenerator;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] bool gpuGeneration;

    [Header("Compute")]
    [SerializeField] ComputeShader noiseCompute;
    [SerializeField] ComputeShader marchingCubesCompute;

    float[] map;

    public void GenerateTerrain()
    {
        if (mapSize == 0 || mapSize % 8 != 0) return;

        map = new float[mapSize * mapSize * mapSize];

        GenerateNoise();
        
        if (generateMesh)
        {
            if (gpuGeneration) GenerateMesh();
            else meshGenerator.GenerateMesh(map, squareSize, surfaceLevel, mapSize);
        }
    }

    private void GenerateNoise()
    {
        ComputeBuffer mapBuffer = new(map.Length, sizeof(float));
        mapBuffer.SetData(map);

        System.Random rand = new(seed);
        int offsetX = rand.Next(0, 100000);
        int offsetY = rand.Next(0, 100000);
        int offsetZ = rand.Next(0, 100000);

        noiseCompute.SetBuffer(0, "map", mapBuffer);
        noiseCompute.SetFloat("scale", scale);
        noiseCompute.SetInt("octaves", octaves);
        noiseCompute.SetFloat("lacunarity", lacunarity);
        noiseCompute.SetFloat("persistence", persistence);
        noiseCompute.SetFloat("weightMultiplier", weightMultiplier);
        noiseCompute.SetInt("mapSize", mapSize);
        noiseCompute.SetInts("noiseOffset", offsetX, offsetY, offsetZ);

        noiseCompute.Dispatch(0, mapSize / 8, mapSize / 8, mapSize / 8);

        mapBuffer.GetData(map);
        mapBuffer.Dispose();
    }

    // Source partially by Sebastion Lague https://github.com/SebLague/Marching-Cubes/blob/master/Assets/Scripts/MeshGenerator.cs
    private void GenerateMesh()
    {
        int numVoxelsPerAxis = mapSize - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        ComputeBuffer mapBuffer = new(map.Length, sizeof(float));
        ComputeBuffer trianglesBuffer = new(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new(1, sizeof(int), ComputeBufferType.Raw);
        trianglesBuffer.SetCounterValue(0);

        marchingCubesCompute.SetBuffer(0, "map", mapBuffer);
        marchingCubesCompute.SetBuffer(0, "triangles", trianglesBuffer);
        marchingCubesCompute.SetFloat("surfaceLevel", surfaceLevel);
        marchingCubesCompute.SetInt("mapSize", mapSize);
        marchingCubesCompute.SetFloat("squareSize", squareSize);

        marchingCubesCompute.Dispatch(0, mapSize / 8, mapSize / 8, mapSize / 8);

        ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        Triangle[] tris = new Triangle[numTris];
        trianglesBuffer.GetData(tris, 0, 0, numTris);

        mapBuffer.Dispose();
        trianglesBuffer.Dispose();
        triCountBuffer.Dispose();

        Mesh mesh = new();

        Vector3[] vertices = new Vector3[numTris * 3];
        int[] triangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                triangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}

struct Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Vector3 this[int i]
    {
        get
        {
            return i switch
            {
                0 => a,
                1 => b,
                _ => c,
            };
        }
    }
}
