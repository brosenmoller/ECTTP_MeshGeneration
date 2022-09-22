using Unity.VisualScripting.ReorderableList;
using UnityEngine;

// Based on tutorial by Sebastian Lague https://www.youtube.com/watch?v=4RpVBYW1r5M
public static class TerrainMeshGenerator
{
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };
    public static readonly int[] supportedFlatShadedChunkSizes = { 48, 72, 96 };

    public static MeshData GenerateTerrainMesh(float[,] heigthMap, int levelOfDetail, bool useFlatShading)
    {
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heigthMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        // To spawn the map in the center
        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;


        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new(verticesPerLine, useFlatShading);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int z = 0; z < borderedSize; z += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement) 
            {
                bool isBorderVertex = z == 0 || z == borderedSize - 1 || x == 0 || x == borderedSize - 1;
                if (isBorderVertex)
                {
                    vertexIndicesMap[x, z] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, z] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int z = 0; z < borderedSize; z += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, z];

                Vector2 percent = new((x - meshSimplificationIncrement) / (float)meshSize, (z - meshSimplificationIncrement) / (float)meshSize);
                Vector3 vertexPosition = new(
                    topLeftX + percent.x * meshSizeUnsimplified, 
                    heigthMap[x, z], 
                    topLeftZ - percent.y * meshSizeUnsimplified
                );

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                // Ignore bottom and right side
                if (x < borderedSize - 1 && z < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, z];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, z];
                    int c = vertexIndicesMap[x, z + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, z + meshSimplificationIncrement];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }

        meshData.CalculateShading();

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    bool useFlatShading;

    public MeshData(int verticesPerLine, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        vertices = new Vector3[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
        uvs = new Vector2[verticesPerLine * verticesPerLine];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[verticesPerLine * 6 * 4];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            // Border
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            // Mesh
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            // Border
            borderTriangles[borderTriangleIndex + 0] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            // Mesh
            triangles[triangleIndex + 0] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointA - pointB;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void CalculateShading()
    {
        if (useFlatShading) FlatShading();
        else BakeNormals();
    }

    void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];


        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs,
        };

        if (useFlatShading) mesh.RecalculateNormals();
        else mesh.normals = bakedNormals;

        return mesh;
    }
}
