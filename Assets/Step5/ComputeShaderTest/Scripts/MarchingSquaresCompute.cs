using UnityEngine;
using System.Collections.Generic;

// Tutorial by Sebastian Lague: https://www.youtube.com/watch?v=yOgIncKp0BE
public class MarchingSquaresCompute : MonoBehaviour
{
    [Header("General")]
    [SerializeField] MeshFilter topMeshFilter;
    [SerializeField] MeshFilter wallMeshFilter;
    float wallHeight;

    // topMesh
    SquareGrid squareGrid;
    List<Vector3> vertices = new();
    List<int> triangles = new();

    // wallMesh
    Dictionary<int, List<Triangle>> triangleDictionary = new();
    List<List<int>> outlines = new();
    HashSet<int> checkedVertices = new();

    public void GenerateMesh(int[] map, float squareSize, float wallHeight, int mapSize)
    {
        this.wallHeight = wallHeight;

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        vertices.Clear();
        triangles.Clear();

        squareGrid = new SquareGrid(map, squareSize, mapSize);

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();
        topMeshFilter.mesh = mesh;

        CreateWallMesh();
    }

    void CreateWallMesh()
    {
        CalculateMeshOutLines();

        List<Vector3> wallVertices = new();
        List<int> wallTriangles = new();

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        Mesh wallMesh = new()
        {
            vertices = wallVertices.ToArray(),
            triangles = wallTriangles.ToArray()
        };

        wallMeshFilter.mesh = wallMesh;
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1: MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft); break;
            case 2: MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight); break;
            case 4: MeshFromPoints(square.topRight, square.centreRight, square.centreTop); break;
            case 8: MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft); break;

            // 2 points:
            case 3: MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft); break;
            case 6: MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom); break;
            case 9: MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft); break;
            case 12: MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft); break;
            case 5: MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft); break;
            case 10: MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft); break;

            // 3 point:
            case 7: MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft); break;
            case 11: MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft); break;
            case 13: MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft); break;
            case 14: MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft); break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3) CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4) CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5) CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6) CreateTriangle(points[0], points[4], points[5]);
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            triangleDictionary.Add(vertexIndexKey, new List<Triangle>() { triangle });
        }
    }

    void CalculateMeshOutLines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlinesVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlinesVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new() { vertexIndex };
                    outlines.Add(newOutline);
                    FollowOutline(newOutlinesVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertexIndex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertexIndex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertexIndex[i];
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutLineEdge(vertexIndex, vertexB)) return vertexB;
                }
            }
        }

        return -1;
    }

    bool IsOutLineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) break;
            }
        }

        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3] { a, b, c };
        }

        public int this[int i] => vertices[i];

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[] map, float squareSize, int nodeCount)
        {
            float mapWidth = nodeCount * squareSize;
            float mapHeight = nodeCount * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCount, nodeCount];

            for (int x = 0; x < nodeCount; x++)
            {
                for (int y = 0; y < nodeCount; y++)
                {
                    Vector3 pos = new(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x + y * nodeCount] == 1, squareSize);
                }
            }

            squares = new Square[nodeCount - 1, nodeCount - 1];
            for (int x = 0; x < nodeCount - 1; x++)
            {
                for (int y = 0; y < nodeCount - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }

        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active) configuration += 8;
            if (topRight.active) configuration += 4;
            if (bottomRight.active) configuration += 2;
            if (bottomLeft.active) configuration += 1;
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

        public bool active;
        public Node above, right;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            this.active = active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }

    }

}

