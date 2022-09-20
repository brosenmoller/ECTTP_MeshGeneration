using UnityEngine;
using System.Collections.Generic;
using System;
// Credit Sebastian Lague: https://www.youtube.com/watch?v=xlSkYjiE-Ck&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=7&t=44s

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 1f;

    const float viewerMoveThresholdForChunkUpdate = 25;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    [Header("Viewer")]
    public Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 lastViewerPostion;
    
    [Header("Looks")]
    public Material material;
    public GameObject water;

    [Header("Level Of Detail")]
    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    static TerrainGenerator terrainGenerator;

    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();

        maxViewDistance = detailLevels[^1].visibleDistanceThreshold;
        chunkSize = TerrainGenerator.terrainChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((lastViewerPostion - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            lastViewerPostion = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDict.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(
                        viewedChunkCoord, chunkSize, detailLevels, transform, material, water, chunkSize
                    ));
                }

            }
        }
    }

    public class TerrainChunk 
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        TerrainData terrainData;
        bool terrainDataReceived;

        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material, GameObject water, float chunkSize)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;

            GameObject waterObject = Instantiate(water, new Vector3(position.x, 2.6f, position.y), Quaternion.identity);
            waterObject.transform.localScale = new Vector3(chunkSize / 7.5f, 1, chunkSize / 7.5f);
            waterObject.transform.parent = meshObject.transform;

            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }

            terrainGenerator.RequestTerrainData(position, OnTerrainDataReceived);
        }

        void OnTerrainDataReceived(TerrainData terrainData)
        {
            this.terrainData = terrainData;
            terrainDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!terrainDataReceived) return;

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
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

                if (lodIndex == 0)
                {
                    if (collisionLODMesh.hasMesh)
                    {
                        meshCollider.sharedMesh = collisionLODMesh.mesh;
                    }
                    else if (!collisionLODMesh.hasRequestedMesh)
                    {
                        collisionLODMesh.RequestMesh(terrainData);
                    }
                }

                terrainChunksVisibleLastUpdate.Add(this);
            }

            SetVisible(visible);
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
        public bool hasRequestedMesh;
        public bool hasMesh;
        public int lod;

        Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            hasMesh = true;
            mesh = meshData.CreateMesh();

            updateCallback();
        }

        public void RequestMesh(TerrainData terrainData)
        {
            hasRequestedMesh = true;
            terrainGenerator.RequestMeshData(terrainData, lod, OnMeshDataReceived);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
        public bool useForCollider;
    }

}
