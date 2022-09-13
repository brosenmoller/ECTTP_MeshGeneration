using UnityEngine;

// Followed tutorial by brackeys https://www.youtube.com/watch?v=eJEpeUH1EMg

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField][Range(1, 100)] int xSize;
    [SerializeField][Range(1, 100)] int zSize;

    [Header("Perlin Noise")]
    [SerializeField][Range(0, 30)] float maxHeigth;
    [SerializeField][Range(.1f, 20)] float scale;

    [Header("Offset Moving")]
    [SerializeField][Range(.001f, .1f)] float scrollSpeed;

    float offsetX, offsetZ;

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // PerlinNoise offset for randomnes
            offsetX = Random.Range(0f, 9999f);
            offsetZ = Random.Range(0f, 9999f);

            CreateShape();
            UpdateMesh();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            offsetX += scrollSpeed;
            offsetZ += scrollSpeed;

            CreateShape();
            UpdateMesh();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            offsetX -= scrollSpeed;
            offsetZ -= scrollSpeed;

            CreateShape();
            UpdateMesh();
        }
    }

    void CreateShape()
    {
        CreateVertices();
        CreateTriangles();
        CreateUV();
    }

    void CreateVertices()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = CalculatePerlinNoise(x, z, scale) * maxHeigth;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
    }

    void CreateTriangles()
    {
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }
    }

    void CreateUV()
    {
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);

                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    float CalculatePerlinNoise(float x, float z, float scale)
    {
        // Calculate PerlinNoise coordinates based on scale and offset
        float xCoord = x / xSize * scale + offsetX;
        float zCoord = z / zSize * scale + offsetZ;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }
}

