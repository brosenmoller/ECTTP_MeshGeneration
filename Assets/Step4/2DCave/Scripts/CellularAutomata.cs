using UnityEngine;

// Tutorial by Sebastian Lague: https://www.youtube.com/watch?v=v7yyZZjF1z4&t=1166s
public class CellularAutomata : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int seed;

    [Header("Walls")]
    [SerializeField][Range(0, 100)] int randomFillPercent;
    [SerializeField][Range(0, 10)] int smoothInterations;
    [SerializeField][Range(0, 9)] int wallCuttoffUpper;
    [SerializeField][Range(0, 9)] int wallCuttoffLower;
    [SerializeField] int borderSize = 5;
    [SerializeField] float wallHeight;

    [Header("Mesh Generation")]
    [SerializeField] float squareSize = 1;
    [SerializeField] bool generateMesh;

    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        for (int i = 0; i < smoothInterations; i++)
        {
            SmoothMap();
        }

        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
        
        if (generateMesh)
        {
            MarchingSquaresMesh meshGen = GetComponent<MarchingSquaresMesh>();
            meshGen.GenerateMesh(borderedMap, squareSize, wallHeight);
        }
    }

    void RandomFillMap()
    {
        System.Random rand = new(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = rand.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap() {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int surroundingWallCount = SurroundingWallCount(x, y);

                if (surroundingWallCount > wallCuttoffUpper)
                {
                    map[x, y] = 1;
                }
                else if (surroundingWallCount < wallCuttoffLower)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int SurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        for (int nextX = gridX - 1; nextX <= gridX + 1; nextX++)
        {
            for (int nextY = gridY - 1; nextY <= gridY + 1; nextY++)
            {
                if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
                {
                    if (nextX != gridX || nextY != gridY)
                    {
                        wallCount += map[nextX, nextY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    void OnDrawGizmos()
    {
        if (map != null && !generateMesh)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = map[x, y] == 1 ? Color.black : Color.green;
                    Vector3 position = new(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(position, Vector3.one);
                }
            }
        }
    }

    private void OnValidate()
    {
        if (wallCuttoffUpper < wallCuttoffLower) wallCuttoffLower = wallCuttoffUpper;
    }
}
