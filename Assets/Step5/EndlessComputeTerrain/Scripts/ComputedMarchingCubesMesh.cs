using System.Collections.Generic;
using UnityEngine;

public class ComputedMarchingCubesMesh : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;

    CubeGrid cubeGrid;
    List<Vector3> vertices = new();
    List<int> triangles = new();

    float surfaceLevel;

    public void GenerateMesh(float[] map, float squareSize, float surfaceLevel, int mapSize)
    {
        this.surfaceLevel = surfaceLevel;

        vertices.Clear();
        triangles.Clear();

        cubeGrid = new CubeGrid(map, squareSize, mapSize);

        for (int x = 0; x < mapSize - 1; x++)
        {
            for (int y = 0; y < mapSize - 1; y++)
            {
                for (int z = 0; z < mapSize - 1; z++)
                {
                    TriangulateCube(cubeGrid.cubes[x, y, z]);
                }
            }
        }

        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        InvertTriangles(mesh);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void InvertTriangles(Mesh mesh)
    {
        //C# or UnityScript
        int[] indices = mesh.triangles;
        int triangleCount = indices.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int tmp = indices[i * 3];
            indices[i * 3] = indices[i * 3 + 1];
            indices[i * 3 + 1] = tmp;
        }
        mesh.triangles = indices;
    }

    void TriangulateCube(Cube cube)
    {
        int cubeIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cube.corners[i].isolevel < surfaceLevel)
            {
                cubeIndex |= 1 << i;
            }
        }

        int[] triangulation = MarchingCubesTriTable.triangulation[cubeIndex];

        foreach (int edgeIndex in triangulation)
        {
            if (edgeIndex == -1) break;

            if (cube.edges[edgeIndex].vertexIndex == -1)
            {
                cube.edges[edgeIndex].vertexIndex = vertices.Count;

                // Interpolation
                int cornerIndexA = MarchingCubesTriTable.cornerIndexAFromEdge[edgeIndex];
                int cornerIndexB = MarchingCubesTriTable.cornerIndexBFromEdge[edgeIndex];

                cube.edges[edgeIndex].position = VertexInterpolate(cube.corners[cornerIndexA], cube.corners[cornerIndexB]);
                // end interpolation

                vertices.Add(cube.edges[edgeIndex].position);
            }

            triangles.Add(cube.edges[edgeIndex].vertexIndex);
        }
    }

    Vector3 VertexInterpolate(ControlNode corner1, ControlNode corner2)
    {
        float mu;
        Vector3 interpolatedVector;

        if (Mathf.Abs(surfaceLevel - corner1.isolevel) < 0.00001)
            return corner1.position;
        if (Mathf.Abs(surfaceLevel - corner2.isolevel) < 0.00001)
            return corner2.position;
        if (Mathf.Abs(corner1.isolevel - corner2.isolevel) < 0.00001)
            return corner1.position;

        mu = (surfaceLevel - corner1.isolevel) / (corner2.isolevel - corner1.isolevel);
        interpolatedVector.x = corner1.position.x + mu * (corner2.position.x - corner1.position.x);
        interpolatedVector.y = corner1.position.y + mu * (corner2.position.y - corner1.position.y);
        interpolatedVector.z = corner1.position.z + mu * (corner2.position.z - corner1.position.z);

        return interpolatedVector;
    }

    public class CubeGrid
    {
        public Cube[,,] cubes;

        public CubeGrid(float[] map, float squareSize, int mapSize)
        {
            float mapDimension = mapSize * squareSize;

            ControlNode[,,] controlNodes = new ControlNode[mapSize, mapSize, mapSize];

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    for (int z = 0; z < mapSize; z++)
                    {
                        Vector3 pos = new(
                            -mapDimension / 2 + x * squareSize + squareSize / 2,
                            -mapDimension / 2 + y * squareSize + squareSize / 2,
                            -mapDimension / 2 + z * squareSize + squareSize / 2
                        );

                        float isoLevel = map[x + y * mapSize + z * mapSize * mapSize];
                        if (x == mapSize - 1 || y == mapSize - 1 || z == mapSize - 1 || x == 0 || y == 0 || z == 0)
                        {
                            isoLevel = 1;
                        }

                        controlNodes[x, y, z] = new ControlNode(pos, isoLevel, squareSize);
                    }
                }
            }

            cubes = new Cube[mapSize - 1, mapSize - 1, mapSize - 1];
            for (int x = 0; x < mapSize - 1; x++)
            {
                for (int y = 0; y < mapSize - 1; y++)
                {
                    for (int z = 0; z < mapSize - 1; z++)
                    {
                        cubes[x, y, z] = new Cube(
                            controlNodes[x, y + 1, z], // front top left
                            controlNodes[x + 1, y + 1, z], // front top right
                            controlNodes[x + 1, y, z], // front bottom right
                            controlNodes[x, y, z], // front bottom left

                            controlNodes[x, y + 1, z + 1], // back top left
                            controlNodes[x + 1, y + 1, z + 1], // back top right
                            controlNodes[x + 1, y, z + 1], // back bottom right
                            controlNodes[x, y, z + 1] // back bottom left
                        );
                    }
                }
            }

        }
    }

    public class Cube
    {
        public ControlNode[] corners = new ControlNode[8];
        public Node[] edges = new Node[12];

        public Cube(ControlNode topFrontLeft, ControlNode topFrontRight, ControlNode bottomFrontRight, ControlNode bottomFrontLeft, ControlNode topBackLeft, ControlNode topBackRight, ControlNode bottomBackRight, ControlNode bottomBackLeft)
        {
            // Corners
            corners[0] = bottomBackLeft;
            corners[1] = bottomBackRight;
            corners[2] = bottomFrontRight;
            corners[3] = bottomFrontLeft;
            corners[4] = topBackLeft;
            corners[5] = topBackRight;
            corners[6] = topFrontRight;
            corners[7] = topFrontLeft;

            // Edges
            edges[0] = bottomBackLeft.right;
            edges[1] = bottomFrontRight.forward;
            edges[2] = bottomFrontLeft.right;
            edges[3] = bottomFrontLeft.forward;

            edges[4] = topBackLeft.right;
            edges[5] = topFrontRight.forward;
            edges[6] = topFrontLeft.right;
            edges[7] = topFrontLeft.forward;

            edges[8] = bottomBackLeft.above;
            edges[9] = bottomBackRight.above;
            edges[10] = bottomFrontRight.above;
            edges[11] = bottomFrontLeft.above;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 pos)
        {
            position = pos;
        }
    }

    public class ControlNode : Node
    {
        public float isolevel;
        public Node above, right, forward;

        public ControlNode(Vector3 pos, float isolevel, float squareSize) : base(pos)
        {
            this.isolevel = isolevel;
            forward = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
            above = new Node(position + Vector3.up * squareSize / 2f);
        }
    }
}

