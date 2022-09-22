using UnityEngine;
using System.Collections;

// Followed tutorial by brackeys https://www.youtube.com/watch?v=eJEpeUH1EMg

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    [SerializeField] [Range(1, 100)] int xSize;
    [SerializeField] [Range(1, 100)] int zSize;

    [Header("Perlin Noise")]
    [SerializeField] [Range(0, 30)] float maxHeigth;
    [SerializeField] [Range(.1f, 20)] float scale;

    [Header("Offset Moving")]
    [SerializeField] [Range(-.1f, .1f)] float scrollSpeed;

    float offsetX, offsetZ;

    MeshFilter meshFilter;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    [HideInInspector] public bool isScrolling;

    public void DrawMesh()
    {
        //// PerlinNoise offset for randomnes
        //offsetX = Random.Range(0f, 9999f);
        //offsetZ = Random.Range(0f, 9999f);

        CreateShape();
        UpdateMesh();
    }

    public void Scroll()
    {
        isScrolling = !isScrolling;
        if (isScrolling)
        {
            StartCoroutine(Scrolling());
        }
        else
        {
            StopAllCoroutines();
        }
    }

    IEnumerator Scrolling()
    {
        while (true)
        {
            offsetX += scrollSpeed;
            offsetZ += scrollSpeed;

            CreateShape();
            UpdateMesh();

            yield return null;
        }
    }

    private void Update()
    {
        if (isScrolling)
        {
            Scroll();
        }
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = CalculatePerlinNoise(x, z) * maxHeigth;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

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
        // Generate UV map for overlaying images
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
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles
        };

        if (uvs != null) mesh.uv = uvs;

        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
    }

    float CalculatePerlinNoise(float x, float z)
    {
        // Calculate PerlinNoise coordinates based on scale and offset
        float xCoord = x / xSize * scale + offsetX;
        float zCoord = z / zSize * scale + offsetZ;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null) return;

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], .1f);
    //    }
    //}
}
