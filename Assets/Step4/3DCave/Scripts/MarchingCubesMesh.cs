using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesMesh : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;

    CubeGrid cubeGrid;
    List<Vector3> vertices = new();
    List<int> triangles = new();

    public void GenerateMesh(int[,,] map, float squareSize)
    {
        vertices.Clear();
        triangles.Clear();

        cubeGrid = new CubeGrid(map, squareSize);

        for (int x = 0; x < cubeGrid.cubes.GetLength(0); x++)
        {
            for (int y = 0; y < cubeGrid.cubes.GetLength(1); y++)
            {
                for (int z = 0; z < cubeGrid.cubes.GetLength(2); z++)
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
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void TriangulateCube(Cube cube)
    {
        int cubeIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cube.backBottomRight.active)
            {
                cubeIndex |= 1 << i;
            }
        }

        int[] triangulation = MarchingCubesTriTable.triangulation[cubeIndex];

        foreach (int edgeIndex in triangulation)
        {
            int indexA = MarchingCubesTriTable.cornerIndexAFromEdge[edgeIndex];
            int indexB = MarchingCubesTriTable.cornerIndexBFromEdge[edgeIndex];

            Vector3 vertexPos = (cube.corners[indexA].position + cube.corners[indexB].position) / 2;

            vertices.Add(vertexPos);
        }

        //if (cube.backBottomLeft.active) cubeindex |= 1; // vertex 0
        //if (cube.backBottomRight.active) cubeindex |= 2; // vertex 1
        //if (cube.frontBottomRight.active) cubeindex |= 4; // vertex 2
        //if (cube.frontBottomLeft.active) cubeindex |= 8; // vertex 3
        //if (cube.backBottomLeft.active) cubeindex |= 16; // vertex 4
        //if (cube.backBottomRight.active) cubeindex |= 32; // vertex 5
        //if (cube.frontTopRight.active) cubeindex |= 64; // vertex 6
        //if (cube.frontTopLeft.active) cubeindex |= 128; // vertex 7

        //Vector3[] vertlist = new Vector3[12];

        //if ()
        //    if (MarchingCubesTriTable.edges[cubeindex] & 1)
        //        vertlist[0] = VertexInterpolate(cube.backBottomLeft, cube.backBottomRight);
        //if (MarchingCubesTriTable.edges[cubeindex] & 2)
        //    vertlist[1] = VertexInterpolate(isolevel, grid.p[1], grid.p[2], grid.val[1], grid.val[2]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 4)
        //    vertlist[2] = VertexInterpolate(isolevel, grid.p[2], grid.p[3], grid.val[2], grid.val[3]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 8)
        //    vertlist[3] = VertexInterpolate(isolevel, grid.p[3], grid.p[0], grid.val[3], grid.val[0]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 16)
        //    vertlist[4] = VertexInterpolate(isolevel, grid.p[4], grid.p[5], grid.val[4], grid.val[5]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 32)
        //    vertlist[5] = VertexInterpolate(isolevel, grid.p[5], grid.p[6], grid.val[5], grid.val[6]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 64)
        //    vertlist[6] = VertexInterpolate(isolevel, grid.p[6], grid.p[7], grid.val[6], grid.val[7]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 128)
        //    vertlist[7] = VertexInterpolate(isolevel, grid.p[7], grid.p[4], grid.val[7], grid.val[4]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 256)
        //    vertlist[8] = VertexInterpolate(isolevel, grid.p[0], grid.p[4], grid.val[0], grid.val[4]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 512)
        //    vertlist[9] = VertexInterpolate(isolevel, grid.p[1], grid.p[5], grid.val[1], grid.val[5]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 1024)
        //    vertlist[10] = VertexInterpolate(isolevel, grid.p[2], grid.p[6], grid.val[2], grid.val[6]);
        //if (MarchingCubesTriTable.edges[cubeindex] & 2048)
        //    vertlist[11] = VertexInterpolate(isolevel, grid.p[3], grid.p[7], grid.val[3], grid.val[7]);

        //int ntriang = 0;
        //for (int i = 0; MarchingCubesTriTable.triangulation[cubeindex][i] != -1; i += 3)
        //{
        //    triangles[ntriang].p[0] = vertlist[triTable[cubeindex][i]];
        //    triangles[ntriang].p[1] = vertlist[triTable[cubeindex][i + 1]];
        //    triangles[ntriang].p[2] = vertlist[triTable[cubeindex][i + 2]];
        //    ntriang++;
        //}

        //return ntriang;

        //int[] points = MarchingCubesTriTable.triangulation[cube.configuration];

        //int count = -1;
        //for (int i = 0; i < points.Length; i++)
        //{
        //    if (points[i] != -1) count++;
        //    else break;
        //}

        //List<Node> nodes = new();

        //for (int i = 0; i < points.Length; i++)
        //{
        //    if (points[i] == -1) break;

        //    switch (points[i])
        //    {
        //        case 0: nodes.Add(cube.backCentreBottom); break;
        //        case 1: nodes.Add(cube.middleCentreBottomRight); break;
        //        case 2: nodes.Add(cube.frontCentreBottom); break;
        //        case 3: nodes.Add(cube.middleCentreBottomLeft); break;
        //        case 4: nodes.Add(cube.backCentreTop); break;
        //        case 5: nodes.Add(cube.middleCentreTopRight); break;
        //        case 6: nodes.Add(cube.frontCentreTop); break;
        //        case 7: nodes.Add(cube.middleCentreTopLeft); break;
        //        case 8: nodes.Add(cube.backCentreLeft); break;
        //        case 9: nodes.Add(cube.backCentreRight); break;
        //        case 10: nodes.Add(cube.frontCentreRight); break;
        //        case 11: nodes.Add(cube.frontCentreLeft); break;
        //    }
        //}
        //MeshFromPoints(nodes.ToArray());
    }

    Node VertexInterpolate(Node p1, Node p2)
    {
        float mu;
        Vector3 p;

        if (p1.position == Vector3.zero)
            return p1;
        if (p2.position == Vector3.zero)
            return p2;
        if (p1.position == p2.position)
            return p1;

        mu = (isolevel - valp1) / (valp2 - valp1);
        p.x = p1.x + mu * (p2.x - p1.x);
        p.y = p1.y + mu * (p2.y - p1.y);
        p.z = p1.z + mu * (p2.z - p1.z);

        return p;
    }

    void MeshFromPoints(Node[] points)
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
    }

    public class CubeGrid
    {
        public Cube[,,] cubes;

        public CubeGrid(int[,,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            int nodeCountZ = map.GetLength(2);

            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;
            float mapDepth = nodeCountZ * squareSize;

            ControlNode[,,] controlNodes = new ControlNode[nodeCountX, nodeCountY, nodeCountZ];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                { 
                    for (int z = 0; z < nodeCountZ; z++)
                    {
                        Vector3 pos = new(
                            -mapWidth / 2 + x * squareSize + squareSize / 2, 
                            -mapHeight / 2 + y * squareSize + squareSize / 2,
                            -mapDepth / 2 + z * squareSize + squareSize / 2
                        );

                        bool active;
                        if (x == nodeCountX - 1 || y == nodeCountY - 1 || z == nodeCountZ - 1 || x == 0 || y == 0 || z == 0)
                        {
                            active = map[x, y, z] == 1;
                        }
                        else
                        {
                            active = map[x, y, z] == 1 &&
                                   !(map[x + 1, y, z] == 1 && map[x, y + 1, z] == 1 && map[x, y, z + 1] == 1 &&
                                     map[x - 1, y, z] == 1 && map[x, y - 1, z] == 1 && map[x, y, z - 1] == 1);
                        }
                        

                        controlNodes[x, y, z] = new ControlNode(pos, active, squareSize);
                    }
                }
            }

            cubes = new Cube[nodeCountX - 1, nodeCountY - 1, nodeCountZ  - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    for (int z = 0; z < nodeCountZ - 1; z++)
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
        public ControlNode frontTopLeft, frontTopRight, frontBottomRight, frontBottomLeft,
                           backTopLeft, backTopRight, backBottomRight, backBottomLeft;

        public Node frontCentreTop, frontCentreRight, frontCentreBottom, frontCentreLeft,
                    middleCentreTopLeft, middleCentreTopRight, middleCentreBottomLeft, middleCentreBottomRight,
                    backCentreTop, backCentreRight, backCentreBottom, backCentreLeft;

        public ControlNode[] corners  = new ControlNode[8];

        public int configuration;

        public Cube(ControlNode frontTopLeft, ControlNode frontTopRight, ControlNode frontBottomRight, ControlNode frontBottomLeft, ControlNode backTopLeft, ControlNode backTopRight, ControlNode backBottomRight, ControlNode backBottomLeft)
        {
            // Corners
            this.backBottomLeft = corners[0] = backBottomLeft;
            this.backBottomRight = corners[1] = backBottomRight;
            this.frontBottomRight = corners[2] = frontBottomRight;
            this.frontBottomLeft = corners[3] = frontBottomLeft;
            this.backTopLeft = corners[4] = backTopLeft;
            this.backTopRight = corners[5] = backTopRight;
            this.frontTopRight = corners[6] = frontTopRight;
            this.frontTopLeft = corners[7] = frontTopLeft;

            // Centre of edges
            frontCentreTop = frontTopLeft.right;
            frontCentreRight = frontBottomRight.above;
            frontCentreBottom = frontBottomLeft.right;
            frontCentreLeft = frontBottomLeft.above;

            middleCentreTopLeft = frontTopLeft.forward;
            middleCentreTopRight = frontTopRight.forward;
            middleCentreBottomLeft = frontBottomLeft.forward;
            middleCentreBottomRight = frontBottomRight.forward;

            backCentreTop = frontTopLeft.right;
            backCentreRight = frontBottomRight.above;
            backCentreBottom = frontBottomLeft.right;
            backCentreLeft = frontBottomLeft.above;

            // Configuration binary
            if (frontTopLeft.active) configuration += 128;
            if (frontTopRight.active) configuration += 64;
            if (frontBottomRight.active) configuration += 32;
            if (frontBottomLeft.active) configuration += 16;
            
            if (backTopLeft.active) configuration += 8;
            if (backTopRight.active) configuration += 4;
            if (backBottomRight.active) configuration += 2;
            if (backBottomLeft.active) configuration += 1;
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
        public Node above, right, forward;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            this.active = active;
            forward = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
            above = new Node(position + Vector3.up * squareSize / 2f);
        }
    }

    private void OnDrawGizmos()
    {
        if (cubeGrid != null)
        {
            for (int x = 0; x < cubeGrid.cubes.GetLength(0); x++)
            {
                for (int y = 0; y < cubeGrid.cubes.GetLength(1); y++)
                {
                    for (int z = 0; z < cubeGrid.cubes.GetLength(2); z++)
                    {


                        Gizmos.color = Color.red;
                        if (cubeGrid.cubes[x, y, z].frontTopLeft.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontTopLeft.position, Vector3.one * .3f);
                        if (cubeGrid.cubes[x, y, z].frontTopRight.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontTopRight.position, Vector3.one * .3f);
                        if (cubeGrid.cubes[x, y, z].frontBottomRight.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontBottomRight.position, Vector3.one * .3f);
                        if (cubeGrid.cubes[x, y, z].frontBottomLeft.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontBottomLeft.position, Vector3.one * .3f);

                        if (cubeGrid.cubes[x, y, z].backTopLeft.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backTopLeft.position, Vector3.one * .3f);
                        if (cubeGrid.cubes[x, y, z].backTopRight.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backTopRight.position, Vector3.one * .3f);
                        if (cubeGrid.cubes[x, y, z].backBottomRight.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backBottomRight.position, Vector3.one * .3f);
                        if (cubeGrid.cubes[x, y, z].backBottomLeft.active)
                            Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backBottomLeft.position, Vector3.one * .3f);

                        //Gizmos.color = Color.grey;
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontCentreTop.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontCentreRight.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontCentreBottom.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].frontCentreLeft.position, Vector3.one * .15f);

                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].middleCentreTopLeft.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].middleCentreTopRight.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].middleCentreBottomLeft.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].middleCentreBottomRight.position, Vector3.one * .15f);

                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backCentreTop.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backCentreRight.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backCentreBottom.position, Vector3.one * .15f);
                        //Gizmos.DrawCube(cubeGrid.cubes[x, y, z].backCentreLeft.position, Vector3.one * .15f);
                    }
                }
            }
        }
    }
}
