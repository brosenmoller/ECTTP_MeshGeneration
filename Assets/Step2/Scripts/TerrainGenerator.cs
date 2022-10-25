using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal.Internal;

// Uses thinks from: https://www.youtube.com/watch?v=f0m73RsBik4&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=8

public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    [SerializeField][Range(0, TerrainMeshGenerator.numSupportedChunkSizes - 1)] int chunkSizeIndex;
    [SerializeField][Range(0, TerrainMeshGenerator.numSupportedLODs - 1)] int EditorlevelOfDetail;

    [Header("Randomization")]
    public int seed;
    [SerializeField] Vector2 offset;

    [Header("Data")]
    public NoiseData noiseData;

    [Header("References")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] Transform naturePrefabParent;

    readonly Queue<TerrainThreadInfo<TerrainData>> terrainDataThreadInfoQueue = new();
    readonly Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    readonly Queue<TerrainThreadInfo<NaturePrefabData>> naturePrefabInfoThreadQueue = new();

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
            return TerrainMeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
        }
    }

    public void GenerateTerrain()
    {
        float[,] heightMap = Noise.GenerateNoiseMap(TerrainChunkSize, TerrainChunkSize, noiseData.scale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, seed, offset, noiseData.heigthCurve, noiseData.maxHeigth, Noise.NormalizeMode.Local).Item1;
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(heightMap, EditorlevelOfDetail);

        Mesh mesh = meshData.CreateMesh();

        meshFilter.sharedMesh = mesh;
        if (meshCollider != null) meshCollider.sharedMesh = mesh;

        if (noiseData.spawnNatureObjects) {
            foreach (Transform child in naturePrefabParent)
            {
                child.gameObject.SetActive(false);
            }

            GenerateNaturePrefabs(Vector2.zero, naturePrefabParent);
        }
    }

    public void GenerateNaturePrefabs(Vector2 center, Transform parent)
    {
        NaturePrefabSpawnable[] spawnables = NaturePrefabGenerator.GenerateNaturePrefabs(seed, TerrainChunkSize, center, noiseData.naturePrefabs);

        foreach (NaturePrefabSpawnable spawnable in spawnables)
        {
            GameObject natureObject = Instantiate(spawnable.prefab, spawnable.position, spawnable.rotation);
            natureObject.transform.localScale = spawnable.scale;
            natureObject.transform.SetParent(parent);
        }
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
        MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(terrainData.heightMap, lod);

        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<MeshData>(callback, meshData));
        }
    } 
    public void RequestNaturePrefabData(Vector2 center, Action<NaturePrefabData> callback)
    {
        ThreadStart threadStart = delegate
        {
            NaturePrefabDataThread(center, callback);
        };

        
        
        new Thread(threadStart).Start();
    }

    void NaturePrefabDataThread(Vector2 center, Action<NaturePrefabData> callback)
    {
        NaturePrefabData naturePrefabData = new(
            NaturePrefabGenerator.GenerateNaturePrefabs(seed, TerrainChunkSize, center, noiseData.naturePrefabs)
        );

        lock (naturePrefabInfoThreadQueue)
        {
            naturePrefabInfoThreadQueue.Enqueue(new TerrainThreadInfo<NaturePrefabData>(callback, naturePrefabData));
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

public struct NaturePrefabData
{
    public NaturePrefabSpawnable[] spawnables;

    public NaturePrefabData(NaturePrefabSpawnable[] spawnables)
    {
        this.spawnables = spawnables;
    }
}


