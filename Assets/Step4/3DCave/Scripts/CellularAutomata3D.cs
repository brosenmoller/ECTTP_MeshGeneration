public static class CellularAutomata3D
{
    static System.Random rand;
    static int mapSize;
    static int[,,] map;

    public static int[,,] GenerateMap(
        int _mapSize, int seed, int smoothInterations, int randomFillPercent, int wallCutoffLower, int wallCutoffUpper
    )
    {
        rand = new(seed);
        mapSize = _mapSize;

        map = new int[mapSize, mapSize, mapSize];

        RandomFillMap(randomFillPercent);
        for (int i = 0; i < smoothInterations; i++)
        {
            SmoothMap(wallCutoffLower, wallCutoffUpper);
        }

        return map;
    }

    static void RandomFillMap(int randomFillPercent)
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    if (x == 0 || x == mapSize - 1 || y == 0 || y == mapSize - 1 || z == 0 || z == mapSize - 1)
                    {
                        map[x, y, z] = 1;
                    }
                    else
                    {
                        map[x, y, z] = rand.Next(0, 100) < randomFillPercent ? 1 : 0;
                    }
                }
            }
        }
    }

    static void SmoothMap(int wallCutoffLower, int wallCutoffUpper)
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    int surroundingWallCount = SurroundingWallCount(x, y, z);

                    if (surroundingWallCount > wallCutoffUpper)
                    {
                        map[x, y, z] = 1;
                    }
                    else if (surroundingWallCount < wallCutoffLower)
                    {
                        map[x, y, z] = 0;
                    }
                }
            }
        }
    }

    static int SurroundingWallCount(int gridX, int gridY, int gridZ)
    {
        int wallCount = 0;

        for (int nextX = gridX - 1; nextX <= gridX + 1; nextX++)
        {
            for (int nextY = gridY - 1; nextY <= gridY + 1; nextY++)
            {
                for (int nextZ = gridZ - 1; nextZ <= gridZ + 1; nextZ++)
                {
                    if (nextX >= 0 && nextX < mapSize && nextY >= 0 && nextY < mapSize && nextZ >= 0 && nextZ < mapSize)
                    {
                        if (nextX != gridX || nextY != gridY || nextZ != gridZ)
                        {
                            wallCount += map[nextX, nextY, nextZ];
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
        }

        return wallCount;
    }
}

