using UnityEngine;

public static class Noise3D
{
    public static int[,,] GenerateMap(int mapSize, float scale, float perlinCutoff, int seed)
    {
        System.Random rand = new(seed);
        int offsetX = rand.Next(0, 100000);
        int offsetY = rand.Next(0, 100000);
        int offsetZ = rand.Next(0, 100000);

        int[,,] map = new int[mapSize, mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    map[x, y, z] = PerlinNoise3D(
                        (x + offsetX) * scale, 
                        (y + offsetY) * scale, 
                        (z + offsetZ) * scale
                    ) >= perlinCutoff ? 1 : 0;
                }
            }
        }

        return map;
    }

    public static float[,,] GenerateMap(int mapSize, float scale, int seed)
    {
        System.Random rand = new(seed);
        int offsetX = rand.Next(0, 100000);
        int offsetY = rand.Next(0, 100000);
        int offsetZ = rand.Next(0, 100000);

        float[,,] map = new float[mapSize, mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    map[x, y, z] = PerlinNoise3D(
                        (x + offsetX) * scale,
                        (y + offsetY) * scale,
                        (z + offsetZ) * scale
                    );
                }
            }
        }

        return map;
    }

    // Source: https://www.youtube.com/watch?v=Aga0TBJkchM:
    public static float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }
}