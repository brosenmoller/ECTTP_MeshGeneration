using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

// Uses thinks from: https://www.youtube.com/watch?v=f0m73RsBik4&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=8

public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    public const int terrainChunkSize = 95;
    public const int editorTerrainChunkSize = 239;
    [SerializeField][Range(0, 6)] int EditorlevelOfDetail;
    [SerializeField] bool useFlatShading;

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

    Queue<TerrainThreadInfo<TerrainData>> terrainDataThreadInfoQueue = new();
    Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new();

    public void GenerateTerrain()
    {
        float[,] heightMap = Noise.GenerateNoiseMap(terrainChunkSize, editorTerrainChunkSize, scale, octaves, persistence, lacunarity, seed, offset, Noise.NormalizeMode.Local);
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(heightMap, gradient, maxHeigth, heigthCurve, EditorlevelOfDetail, useFlatShading);

        Mesh mesh = meshData.CreateMesh();

        meshFilter.sharedMesh = mesh;
        if (meshCollider != null) meshCollider.sharedMesh = mesh;
    }
    public void RequestTerrainData(Vector2 center, Action<TerrainData> callback)
    {
        ThreadStart threadStart = delegate
        {
            TerrainDataThread(center, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void TerrainDataThread(Vector2 center, Action<TerrainData> callback)
    {
        float[,] heightMap = Noise.GenerateNoiseMap(
            terrainChunkSize + 2, terrainChunkSize + 2, scale, octaves, persistence, lacunarity, seed, center + offset, Noise.NormalizeMode.Global
        );

        TerrainData terrainData = new(heightMap);

        lock (terrainDataThreadInfoQueue)
        {
            terrainDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<TerrainData>(callback, terrainData));
        }
    }

    public void RequestMeshData(TerrainData terrainData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(terrainData, lod, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void MeshDataThread(TerrainData terrainData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(terrainData.heightMap, gradient, maxHeigth, heigthCurve, lod, useFlatShading);

        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<MeshData>(callback, meshData));
        }
    }
    

    void Update()
    {
        if (terrainDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < terrainDataThreadInfoQueue.Count; i++)
            {
                TerrainThreadInfo<TerrainData> threadInfo = terrainDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                TerrainThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct TerrainThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public TerrainThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

public struct TerrainData 
{
    public float[,] heightMap;

    public TerrainData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }

}


