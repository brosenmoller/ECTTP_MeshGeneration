using UnityEngine;

// Based on Tutorial by Sebastion Lague - https://www.youtube.com/watch?v=MRNFcywkUSA
public static class Noise 
{
    public enum NormalizeMode { Local, Global };

    public static (float[,], float) GenerateNoiseMap(
        int xSize,
        int zSize,
        float scale,
        int octaves, 
        float persistence, 
        float lacunarity,
        int seed,
        Vector2 offset,
        AnimationCurve _heightCurve,
        float maxHeight,
        NormalizeMode normalizeMode
    )
    {
        AnimationCurve heightCurve = new(_heightCurve.keys);
        float[,] noiseMap = new float[xSize, zSize];

        System.Random random = new(seed);
        Vector2[] octaveOFfsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetZ = random.Next(-100000, 100000) - offset.y;
            octaveOFfsets[i] = new Vector2(offsetX, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        // To scale to the center
        float halfXSize = xSize / 2f;
        float halfZSize = zSize / 2f;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfXSize + octaveOFfsets[i].x) / scale * frequency;
                    float sampleZ = (z - halfZSize + octaveOFfsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                } 
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, z] = noiseHeight;
            }
        }

        float minGlobalNoiseHeight = float.MaxValue;

        // Normalize noisemap
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++) 
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, z] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, z]);
                    noiseMap[x, z] = heightCurve.Evaluate(noiseMap[x, z]) * maxHeight;
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, z] + 1) / (2f * maxPossibleHeight / 2.4f);
                    noiseMap[x, z] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    noiseMap[x, z] = heightCurve.Evaluate(noiseMap[x, z]) * maxHeight;

                    if (noiseMap[x, z] < minGlobalNoiseHeight) minGlobalNoiseHeight = noiseMap[x, z];
                }
            }
        }
        
        return (noiseMap, minGlobalNoiseHeight);
    }
}
