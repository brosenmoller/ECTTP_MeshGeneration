using UnityEngine;

public class ComputedTerrainGenerator : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    public int chunkSize;
    [SerializeField] int seed;

    [Header("Noise")]
    [SerializeField][Range(0, 1)] float surfaceLevel;
    [SerializeField][Range(0, 100)] int offset;
    [SerializeField] float scale;
    [Range(1, 10)] public int octaves;
    [Range(0, 1f)] public float persistence;
    [Range(1, 10)] public float lacunarity;
    [Range(0, 1f)] public float weightMultiplier;

    [Header("Mesh")]
    [SerializeField] bool generateMesh;
    public float squareSize;
    [SerializeField] ComputedMarchingCubesMesh meshGenerator;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] bool gpuGeneration;

    [Header("Compute")]
    [SerializeField] ComputeShader noiseCompute;
    [SerializeField] ComputeShader marchingCubesCompute;

    [Header("Nature Prefabs")]
    [SerializeField] bool spawnNatureObjects;
    [SerializeField] NaturePrefab[] naturePrefabs;

    int offsetX = 0;
    int offsetY = 0;
    int offsetZ = 0;
    System.Random rand;

    public void GenerateTerrain()
    {
        if (chunkSize == 0 || chunkSize % 8 != 0) return;

        float[] map = GenerateNoise(new Vector3Int(offset, 0, 0));
        
        if (generateMesh)
        {
            if (gpuGeneration) GenerateMesh(meshFilter, map);
            else meshGenerator.GenerateMesh(map, squareSize, surfaceLevel, chunkSize);
        }
    }

    public void GenerateNaturePrefabs(Vector2 center, Transform parent, int chunkSize)
    {
        NaturePrefabSpawnable[] spawnables = NaturePrefabGenerator.GenerateNaturePrefabs(seed, chunkSize, center, naturePrefabs);

        foreach (NaturePrefabSpawnable spawnable in spawnables)
        {
            GameObject natureObject = Instantiate(spawnable.prefab, spawnable.position, spawnable.rotation);
            natureObject.transform.localScale = spawnable.scale;
            natureObject.transform.SetParent(parent);
        }
    }

    public float[] GenerateNoise(Vector3Int baseOffset)
    {
        float[] map = new float[chunkSize * chunkSize * chunkSize];

        ComputeBuffer mapBuffer = new(map.Length, sizeof(float));
        mapBuffer.SetData(map);

        rand ??= new(seed);
        if (offsetX == 0)
        {
            offsetX = rand.Next(0, 100000);
            offsetY = rand.Next(0, 100000);
            offsetZ = rand.Next(0, 100000);
        }

        int currentOffsetX = offsetX + baseOffset.x;
        int currentOffsetY = offsetY + baseOffset.y;
        int currentOffsetZ = offsetZ + baseOffset.z;

        noiseCompute.SetBuffer(0, "map", mapBuffer);
        noiseCompute.SetFloat("scale", scale);
        noiseCompute.SetInt("octaves", octaves);
        noiseCompute.SetFloat("lacunarity", lacunarity);
        noiseCompute.SetFloat("persistence", persistence);
        noiseCompute.SetFloat("weightMultiplier", weightMultiplier);
        noiseCompute.SetInt("mapSize", chunkSize);
        noiseCompute.SetInts("noiseOffset", currentOffsetX, currentOffsetY, currentOffsetZ);

        noiseCompute.Dispatch(0, chunkSize / 8, chunkSize / 8, chunkSize / 8);

        mapBuffer.GetData(map);
        mapBuffer.Dispose();

        return map;
    }

    // Source partially by Sebastion Lague https://github.com/SebLague/Marching-Cubes/blob/master/Assets/Scripts/MeshGenerator.cs
    public void GenerateMesh(MeshFilter meshFilter, float[] map)
    {
        int numVoxelsPerAxis = chunkSize - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        ComputeBuffer mapBuffer = new(map.Length, sizeof(float));
        mapBuffer.SetData(map);
        ComputeBuffer trianglesBuffer = new(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new(1, sizeof(int), ComputeBufferType.Raw);
        trianglesBuffer.SetCounterValue(0);

        marchingCubesCompute.SetBuffer(0, "map", mapBuffer);
        marchingCubesCompute.SetBuffer(0, "triangles", trianglesBuffer);
        marchingCubesCompute.SetFloat("surfaceLevel", surfaceLevel);
        marchingCubesCompute.SetInt("mapSize", chunkSize);
        marchingCubesCompute.SetFloat("squareSize", squareSize);

        marchingCubesCompute.Dispatch(0, chunkSize / 8, chunkSize / 8, chunkSize / 8);

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