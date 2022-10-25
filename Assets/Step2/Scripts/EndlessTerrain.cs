using UnityEngine;
using System.Collections.Generic;
using System;

// Credit Sebastian Lague: https://www.youtube.com/watch?v=xlSkYjiE-Ck&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=7&t=44s

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float colliderGenerationDistanceThreshold = 25;

    [Header("Viewer")]
    public Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 lastViewerPostion;
    
    [Header("Looks")]
    public Material material;
    public GameObject water;

    [Header("Level Of Detail")]
    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    static TerrainGenerator terrainGenerator;

    int chunkSize;
    int chunksVisibleInViewDistance;
    readonly Dictionary<Vector2, TerrainChunk> terrainChunkDict = new();
    static readonly List<TerrainChunk> visibleTerrainChunks = new();

    void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();

        maxViewDistance = detailLevels[^1].visibleDistanceThreshold;
        chunkSize = terrainGenerator.TerrainChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / terrainGenerator.noiseData.uniformScale;

        if ((lastViewerPostion - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            lastViewerPostion = viewerPosition;
            UpdateVisibleChunks();
        }

        if (viewerPosition != lastViewerPostion)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.SetCollider();
            }
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDict.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(
                            viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, transform, material, water
                        ));
                    }
                }
            }
        }
    }

    public class TerrainChunk 
    {
        readonly GameObject meshObject;
        readonly MeshRenderer meshRenderer;
        readonly MeshFilter meshFilter;
        readonly MeshCollider meshCollider;

        readonly GameObject water;
        readonly int size;

        readonly LODInfo[] detailLevels;
        readonly LODMesh[] lodMeshes;
        readonly int colliderLODIndex;

        public Vector2 coord;

        Vector2 position;
        Bounds bounds;
        TerrainData terrainData;

        bool terrainDataReceived;
        bool hasSetCollider;
        bool hasCheckedForWater;

        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material, GameObject water)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;
            this.water = water;
            this.size = size;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshObject.tag = "Terrain";
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = positionV3 * terrainGenerator.noiseData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * terrainGenerator.noiseData.uniformScale;

            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex) lodMeshes[i].UpdateCallback += SetCollider;
            }

            terrainGenerator.RequestTerrainData(position, OnTerrainDataReceived);
        }

        void OnTerrainDataReceived(TerrainData terrainData)
        {
            this.terrainData = terrainData;
            terrainDataReceived = true;

            UpdateTerrainChunk();
        }

        void SpawnWater()
        {
            GameObject waterObject = Instantiate(
                water, 
                new Vector3(position.x, terrainGenerator.noiseData.waterLevel, position.y), 
                Quaternion.identity
            );
            waterObject.transform.localScale = new Vector3(size / 7.5f, 1, size / 7.5f);
            waterObject.transform.SetParent(meshObject.transform);
        }

        public void UpdateTerrainChunk()
        {
            if (!terrainDataReceived) return;

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold) lodIndex = i + 1;
                    else break;
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(terrainData);
                    }
                }

                if (!hasCheckedForWater)
                {
                    if (terrainData.lowestPoint < terrainGenerator.noiseData.waterLevel)
                    {
                        SpawnWater();
                    }

                    hasCheckedForWater = true;
                }
            }

            if (wasVisible != visible)
            {
                if (visible) visibleTerrainChunks.Add(this);
                else visibleTerrainChunks.Remove(this);
                SetVisible(visible);
            }
            
        }

        public void SetCollider()
        {
            if (hasSetCollider) return;

            float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].SqrVisibleDistanceThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(terrainData);
                }
            }

            if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                    
                    SpawnNatureObjects();

                }
            }
        }

        void SpawnNatureObjects()
        {
            GameObject naturePrefabParent = new("NaturePrefabs");
            naturePrefabParent.transform.SetParent(meshObject.transform);

            terrainGenerator.GenerateNaturePrefabs(position, naturePrefabParent.transform);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public NaturePrefabData naturePrefabData;
        public int lod;

        public bool hasRequestedMesh;
        public bool hasMesh;

        public bool hasRequestedNaturePrefabs;
        public bool hasNaturePrefabs;

        public event Action UpdateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            hasMesh = true;
            mesh = meshData.CreateMesh();

            UpdateCallback();
        }

        public void RequestMesh(TerrainData terrainData)
        {
            hasRequestedMesh = true;
            terrainGenerator.RequestMeshData(terrainData, lod, OnMeshDataReceived);
        }

        void OnNaturePrefabDataReceived(NaturePrefabData naturePrefabData)
        {
            hasNaturePrefabs = true;
            this.naturePrefabData = naturePrefabData;

            UpdateCallback();
        }

        public void RequestNaturePrefabs(Vector2 center)
        {
            hasRequestedNaturePrefabs = true;
            terrainGenerator.RequestNaturePrefabData(center, OnNaturePrefabDataReceived);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        [Range(0, TerrainMeshGenerator.numSupportedLODs)]
        public int lod;
        public float visibleDistanceThreshold;

        public float SqrVisibleDistanceThreshold
        {
            get { return visibleDistanceThreshold * visibleDistanceThreshold; }
        }
    }

}
