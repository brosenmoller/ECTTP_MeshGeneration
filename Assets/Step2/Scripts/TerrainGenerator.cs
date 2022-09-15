using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    const int terrainChunkSize = 241;
    [SerializeField][Range(0, 6)] int levelOfDetail;

    [Header("Perlin Noise")]
    [SerializeField][Range(0, 100)] float maxHeigth;
    [SerializeField][Range(50, 500)] float scale;
    [SerializeField][Range(1, 10)] int octaves;
    [SerializeField][Range(0, 1f)] float persistence;
    [SerializeField][Range(1, 10)] float lacunarity;
    [SerializeField] AnimationCurve heigthCurve;

    [Header("Randomization")]
    [SerializeField] int seed;
    [SerializeField] Vector2 offset;

    [Header("Colors")]
    [SerializeField] Gradient gradient;

    [Header("References")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;

    public void GenerateTerrain()
    {
        float[,] heightMap = Noise.GenerateNoiseMap(terrainChunkSize, terrainChunkSize, scale, octaves, persistence, lacunarity, seed, offset);
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(heightMap, gradient, maxHeigth, heigthCurve, levelOfDetail);

        Mesh mesh = meshData.CreateMesh();
        meshFilter.sharedMesh = mesh;
        if (meshCollider != null) meshCollider.sharedMesh = mesh;
    }
}

