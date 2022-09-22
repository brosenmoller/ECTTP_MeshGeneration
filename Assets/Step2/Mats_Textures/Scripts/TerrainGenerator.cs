using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

// Uses thinks from: https://www.youtube.com/watch?v=f0m73RsBik4&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=8

public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    [SerializeField][Range(0, TerrainMeshGenerator.numSupportedChunkSizes - 1)] int chunkSizeIndex;
    [SerializeField][Range(0, TerrainMeshGenerator.numSupportedFlatShadedChunkSizes - 1)] int flatShadedChunkSizeIndex;
    [SerializeField][Range(0, TerrainMeshGenerator.numSupportedLODs - 1)] int EditorlevelOfDetail;
    [SerializeField] bool useFlatShading;

    [Header("Randomization")]
    [SerializeField] int seed;
    [SerializeField] Vector2 offset;

    [Header("Data")]
    public GlobalTerrainData globalTerrainData;
    public NoiseData noiseData;

    [Header("Colors")]
    [SerializeField] Gradient gradient;

    [Header("References")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;

    Queue<TerrainThreadInfo<TerrainData>> terrainDataThreadInfoQueue = new();
    Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new();

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            GenerateTerrain();
        }
    }

    public int TerrainChunkSize
    {
        get
        {
            if (globalTerrainData.useFlatShading)
            {
                return TerrainMeshGenerator.supportedFlatShadedChunkSizes[flatShadedChunkSizeIndex] - 1;
            }
            else
            {
                return TerrainMeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
            }
        }
    }

    public void GenerateTerrain()
    {
        float[,] heightMap = Noise.GenerateNoiseMap(TerrainChunkSize, TerrainChunkSize, noiseData.scale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, seed, offset, noiseData.heigthCurve, noiseData.maxHeigth, Noise.NormalizeMode.Local).Item1;
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(heightMap, EditorlevelOfDetail, useFlatShading);

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
        (float[,], float) tuple = Noise.GenerateNoiseMap(
            TerrainChunkSize + 2, TerrainChunkSize + 2, noiseData.scale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, seed, center + offset, noiseData.heigthCurve, noiseData.maxHeigth, Noise.NormalizeMode.Global
        );

        float[,] heightMap = tuple.Item1;
        float minHeight = tuple.Item2;

        TerrainData terrainData = new(heightMap, minHeight);

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
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(terrainData.heightMap, lod, useFlatShading);

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

    private void OnValidate()
    {
        if (globalTerrainData != null)
        {
            globalTerrainData.OnValuesUpdated -= OnValuesUpdated;
            globalTerrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
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
    public float lowestPoint;

    public TerrainData(float[,] heightMap, float lowestPoint)
    {
        this.heightMap = heightMap;
        this.lowestPoint = lowestPoint;
    }

}


