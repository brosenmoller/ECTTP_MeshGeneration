using System;
using System.Collections.Generic;
using UnityEngine;
public static class NaturePrefabGenerator
{
    static System.Random random;

    public static NaturePrefabSpawnable[] GenerateNaturePrefabs(int seed, int chunkSize, Vector2 center, NaturePrefab[] naturePrefabs)
    {
        List<NaturePrefabSpawnable> naturePrefabSpawnables = new();
        random = new System.Random(seed * center.GetHashCode());

        for (int prefabIndex = 0; prefabIndex < naturePrefabs.Length; prefabIndex++)
        {
            NaturePrefab naturePrefab = naturePrefabs[prefabIndex];

            // loop for calulated number based on frequency and the world dimensions
            for (int i = 0; i < (naturePrefab.frequency * chunkSize); i++)
            {
                Vector3 randomPos = GenerateRandomPosition(chunkSize);
                randomPos.x += center.x;
                randomPos.z += center.y;

                // If Raycast doesn't hit anything
                if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 100))
                {
                    float normalDot = Vector3.Dot(hit.normal, Vector3.up);

                    // Check if object can be spawned here
                    if (hit.point.y > naturePrefab.minMaxAltitude.x && hit.point.y < naturePrefab.minMaxAltitude.y &&
                        !(normalDot > naturePrefab.minMaxSteepness.x && normalDot < naturePrefab.minMaxSteepness.y) &&
                        hit.transform.CompareTag("Terrain"))
                    {
                        // Set random rotation
                        Vector3 randomRotation = new(
                            naturePrefab.prefab.transform.rotation.eulerAngles.x, 
                            random.Next(0, 361), 
                            0
                        );

                        // Set the correct y value 
                        randomPos.y = hit.point.y + naturePrefab.prefab.transform.localPosition.y;

                        // Instantiate the gameObject
                        naturePrefabSpawnables.Add(new NaturePrefabSpawnable(
                            naturePrefab.prefab, 
                            randomPos, 
                            Quaternion.Euler(randomRotation), 
                            naturePrefab.prefab.transform.localScale
                        ));
                    }
                }
            }
        }

        return naturePrefabSpawnables.ToArray();
    }

    static Vector3 GenerateRandomPosition(int size) 
    {
        float xCoord = random.Next(-(size / 2), size / 2);
        float zCoord = random.Next(-(size / 2), size / 2);

        return new Vector3(xCoord, 50, zCoord);
    } 
}

[Serializable]
public class NaturePrefab
{
    public GameObject prefab;
    [Range(0, 100)]
    public float frequency;
    public float groupness;
    public Vector2 minMaxAltitude;
    public Vector2 minMaxSteepness;
}

public struct NaturePrefabSpawnable
{
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public NaturePrefabSpawnable(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.prefab = prefab;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
}
