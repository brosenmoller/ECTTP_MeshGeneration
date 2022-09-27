using System;
using UnityEngine;

public class NaturePrefabGenerator : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform parent;

    [Header("Prefabs")]
    [SerializeField] NaturePrefab[] naturePrefabs;

    public void GenerateNaturePrefabs(int seed, int chunkSize)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        for (int prefabIndex = 0; prefabIndex < naturePrefabs.Length; prefabIndex++)
        {
            NaturePrefab naturePrefab = naturePrefabs[prefabIndex];

            // loop for calulated number based on frequency and the world dimensions
            for (int i = 0; i < (naturePrefab.frequency * chunkSize); i++)
            {
                Vector3 randomPos = GenerateRandomPosition(seed, chunkSize, (i + 1) * (prefabIndex + 1));

                // If Raycast doesn't hit anything
                if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 100))
                {
                    // Check if object can be spawned on the this ground type
                    if (hit.point.y > naturePrefab.minMaxAltitude.x && hit.point.y < naturePrefab.minMaxAltitude.y &&
                        hit.transform.CompareTag("Terrain"))
                    {
                        // Set random rotation
                        Vector3 randomRotation = new(
                            naturePrefab.prefab.transform.rotation.eulerAngles.x, 
                            UnityEngine.Random.Range(0, 361), 
                            0
                        );

                        // Set the correct y value 
                        randomPos.y = hit.point.y;

                        // Instantiate the gameObject
                        GameObject lastInstantiated = Instantiate(naturePrefab.prefab, randomPos, Quaternion.Euler(randomRotation));
                        
                        // Set all initial values
                        lastInstantiated.transform.localScale = naturePrefab.prefab.transform.localScale;
                        lastInstantiated.transform.SetParent(parent.transform, true);
                    }
                }
            }
        }
    }

    Vector3 GenerateRandomPosition(int seed, int size, int seedOffset) 
    {
        System.Random random = new(seed + seedOffset.GetHashCode());
        float xCoord = UnityEngine.Random.Range(-(size / 2), size / 2);
        float zCoord = UnityEngine.Random.Range(-(size / 2), size / 2);

        return new Vector3(xCoord, 50, zCoord);
    } 
}

[Serializable]
public class NaturePrefab
{
    public GameObject prefab;
    [Range(0, 1)]
    public float frequency;
    public float groupness;
    public Vector2 minMaxAltitude;
    public Vector2 minMaxSteepness;
}
