using UnityEngine;

// Based on tutorial by Sebastian Lague https://www.youtube.com/watch?v=4RpVBYW1r5M
public static class TerrainMeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heigthMap, Gradient colorGradient, float maxHeigth, AnimationCurve heigthCurve, int levelOfDetail)
    {
        int xSize = heigthMap.GetLength(0);
        int zSize = heigthMap.GetLength(1);

        // To spawn the map in the center
        float topLeftX = (xSize - 1) / -2f;
        float topLeftZ = (zSize - 1) / 2f;

        MeshData meshData = new(xSize, zSize);
        int vertexIndex = 0;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = (xSize - 1) / meshSimplificationIncrement + 1;

        for (int z = 0; z < zSize; z += meshSimplificationIncrement)
        {
            for (int x = 0; x < xSize; x += meshSimplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(
                    topLeftX + x, 
                    heigthCurve.Evaluate(heigthMap[x, z]) * maxHeigth, 
                    topLeftZ - z
                );

                meshData.colors[vertexIndex] = colorGradient.Evaluate(heigthMap[x, z]);

                // Ignore bottom and right side
                if (x < xSize - 1 && z < zSize - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Color[] colors;

    int triangleIndex;

    public MeshData(int xSize, int zSize)
    {
        vertices = new Vector3[xSize * zSize];
        triangles = new int[(xSize - 1) * (zSize - 1) * 6];
        colors = new Color[xSize * zSize];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex + 0] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            colors = colors
        };

        mesh.RecalculateNormals();

        return mesh;
    }
}
