using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    [SerializeField][Range(1, 100)] int xSize;
    [SerializeField][Range(1, 100)] int zSize;

    [Header("Perlin Noise")]
    [SerializeField][Range(0, 100)] float maxHeigth;
    [SerializeField][Range(.1f, 100)] float scale;
    [SerializeField][Range(1, 10)] int octaves;
    [SerializeField][Range(0, 1f)] float persistence;
    [SerializeField][Range(1, 10)] float lacunarity;

    [Header("Randomization")]
    [SerializeField] int seed;
    [SerializeField] Vector2 offset;

    [Header("Colors")]
    [SerializeField] Gradient gradient;

    [Header("References")]
    [SerializeField] MeshFilter meshFilter;

    public void GenerateTerrain()
    {
        float[,] heightMap = Noise.GenerateNoiseMap(xSize, zSize, scale, octaves, persistence, lacunarity, seed, offset);
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(heightMap, gradient, maxHeigth);

        meshFilter.mesh = meshData.CreateMesh();
    }
}

