using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeEndlessTerrain : MonoBehaviour
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
    public static float maxViewDistance = 200;

    static ComputedTerrainGenerator terrainGenerator;

    int chunkSize;
    int chunksVisibleInViewDistance;
    readonly Dictionary<Vector2, TerrainChunk> terrainChunkDict = new();
    static readonly List<TerrainChunk> visibleTerrainChunks = new();

    void Start()
    {
        terrainGenerator = FindObjectOfType<ComputedTerrainGenerator>();

        chunkSize = terrainGenerator.chunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / terrainGenerator.squareSize;

        if ((lastViewerPostion - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            lastViewerPostion = viewerPosition;
            UpdateVisibleChunks();
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
                            viewedChunkCoord, chunkSize, transform, material
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

        public Vector2 coord;

        Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            this.coord = coord;
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

            meshObject.transform.position = positionV3 * terrainGenerator.squareSize;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one;

            SetVisible(false);

            float[] map = terrainGenerator.GenerateNoise(new Vector3Int((int)position.x, 0, (int)position.y));
            terrainGenerator.GenerateMesh(meshFilter, map);

            //terrainGenerator.RequestTerrainData(position, OnTerrainDataReceived);
        }

        //void SpawnWater()
        //{
        //    GameObject waterObject = Instantiate(
        //        water,
        //        new Vector3(position.x, terrainGenerator.noiseData.waterLevel, position.y),
        //        Quaternion.identity
        //    );
        //    waterObject.transform.localScale = new Vector3(size / 7.5f, 1, size / 7.5f);
        //    waterObject.transform.SetParent(meshObject.transform);
        //}

        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if (wasVisible != visible)
            {
                if (visible) visibleTerrainChunks.Add(this);
                else visibleTerrainChunks.Remove(this);
                SetVisible(visible);
            }

        }

        //public void SetCollider()
        //{
        //    if (hasSetCollider) return;

        //    float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

        //    if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].SqrVisibleDistanceThreshold)
        //    {
        //        if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
        //        {
        //            lodMeshes[colliderLODIndex].RequestMesh(terrainData);
        //        }
        //    }

        //    if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
        //    {
        //        if (lodMeshes[colliderLODIndex].hasMesh)
        //        {
        //            meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
        //            hasSetCollider = true;

        //            SpawnNatureObjects();

        //        }
        //    }
        //}

        void SpawnNatureObjects()
        {
            GameObject naturePrefabParent = new("NaturePrefabs");
            naturePrefabParent.transform.SetParent(meshObject.transform);

            terrainGenerator.GenerateNaturePrefabs(position, naturePrefabParent.transform, terrainGenerator.chunkSize - 1);
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
}
