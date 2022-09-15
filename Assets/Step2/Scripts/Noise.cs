using UnityEngine;

// Based on Tutorial by Sebastion Lague - https://www.youtube.com/watch?v=MRNFcywkUSA
public static class Noise 
{
    public static float[,] GenerateNoiseMap(
        int xSize, 
        int zSize,
        float scale,
        int octaves, 
        float persistence, 
        float lacunarity,
        int seed,
        Vector2 offset
    )
    {
        float[,] noiseMap = new float[xSize, zSize];

        System.Random random = new(seed);
        Vector2[] octaveOFfsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetZ = random.Next(-100000, 100000) + offset.y;
            octaveOFfsets[i] = new Vector2(offsetX, offsetZ);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // To scale to the center
        float halfXSize = xSize / 2f;
        float halfZSize = zSize / 2f;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfXSize) / scale * frequency + octaveOFfsets[i].x;
                    float sampleZ = (z - halfZSize) / scale * frequency + octaveOFfsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                } 
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, z] = noiseHeight;
            }
        }

        // Normalize noisemap
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++) 
            {
                noiseMap[x, z] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
            }
        }
        
        return noiseMap;
    }
}
