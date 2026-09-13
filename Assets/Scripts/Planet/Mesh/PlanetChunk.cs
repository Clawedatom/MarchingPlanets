using System.Collections.Generic;
using UnityEngine;

public class PlanetChunk : MonoBehaviour
{
    private PlanetMeshGenerator manager;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    [SerializeField] private Vector3Int chunkCoords;

    //Mesh Fields
    private readonly List<Vector3> meshVerts = new List<Vector3>();
    private readonly List<int> meshTris = new List<int>();
    private readonly List<Color> meshColors = new List<Color>();

    // Reusable buffer to avoid allocations inside MarchCube loop
    private readonly Vector3[] edgeVertex = new Vector3[12];



    private static readonly Vector3Int[] cornerPointOffsets =
    {
        new Vector3Int(0,0,0),
        new Vector3Int(1,0,0),
        new Vector3Int(1,1,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,0,1),
        new Vector3Int(1,0,1),
        new Vector3Int(1,1,1),
        new Vector3Int(0,1,1)
    };

    public void Initialize(PlanetMeshGenerator m, Vector3Int chunkC)
    {
        manager = m;
        chunkCoords = chunkC;

        //Mesh Setup
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        

        GenerateChunkMesh();
    }

    private void GenerateChunkMesh()
    {
        meshVerts.Clear();
        meshTris.Clear();
        meshColors.Clear();

        int chunkSize = manager.ChunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    MarchCube(x, y, z); // Create Tri and Verts based on density values
                }
            }
        }

        //Build chunk Mesh once tris and verts are calcd

        BuildMesh();
    }

    private void MarchCube(int x, int y, int z)
    {
        Vector3Int localPos = new Vector3Int(x, y, z);

        //Retrieve denisty values from manager script

        float[] cubeCorners = new float[8];
        int chunkSize = manager.ChunkSize;
        for (int i = 0; i < 8; i++)
        {
            Vector3Int localCorner = localPos + cornerPointOffsets[i];

            int worldX = chunkCoords.x * chunkSize + localCorner.x;
            int worldY = chunkCoords.y * chunkSize + localCorner.y;
            int worldZ = chunkCoords.z * chunkSize + localCorner.z;

            cubeCorners[i] = manager.GetDensity(worldX, worldY, worldZ);


        }
        //use corner density to get calc index
        int caseIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i] > manager.IsoLevel)
            {
                caseIndex |= (1 << i); // shift bit left if corner is inside surface
            }
        }

        if (caseIndex == 0 || caseIndex == 255) // both indices mean empty - 0 all outside or 255 all inside ( so no need to render)
            return;


        //calc where edge of triangle should be placed along grid

        int edgeMask = MCLookUpTable.edgeTable[caseIndex];

        for (int e = 0; e < 12; e++)
        {
            if ((edgeMask & (1 << e)) == 0)
            {
                continue;
            }

            //edge point
            int cornerA = MCLookUpTable.cornerIndexAFromEdge[e];
            int cornerB = MCLookUpTable.cornerIndexBFromEdge[e];

            //positions of corners
            Vector3 posA = localPos + cornerPointOffsets[cornerA];
            Vector3 posB = localPos + cornerPointOffsets[cornerB];

            


            edgeVertex[e] = Interpolate(posA, posB, cubeCorners[cornerA], cubeCorners[cornerB]); //position of mesh vertex based on density values at either side of vertex

        }


        //Get tris and verts from table baed on case index
        int[] triangles = MCLookUpTable.triangulation[caseIndex];

        for (int i = 0; i < 15; i += 3)
        {
            if (triangles[i] == -1)
                break;

            int triIndex = meshVerts.Count;

            Vector3 v0 = edgeVertex[triangles[i]];
            Vector3 v1 = edgeVertex[triangles[i + 1]];
            Vector3 v2 = edgeVertex[triangles[i + 2]];

            //add vert pos to mesh verts
            meshVerts.Add(v0);
            meshVerts.Add(v1);
            meshVerts.Add(v2);

            //Calculate height color for vertex
            meshColors.Add(GetColourFromHeight(v0));
            meshColors.Add(GetColourFromHeight(v1));
            meshColors.Add(GetColourFromHeight(v2));



            //triangles wound in 0,2,1 orider to flip face normals out
            meshTris.Add(triIndex);
            meshTris.Add(triIndex + 2);
            meshTris.Add(triIndex + 1);

        }
    }

    private Color GetColourFromHeight(Vector3 localVertPos)
    {
        int chunkSize = manager.ChunkSize;

        Vector3 gridPos = new Vector3(
            chunkCoords.x * chunkSize + localVertPos.x,
            chunkCoords.y * chunkSize + localVertPos.y,
            chunkCoords.z * chunkSize + localVertPos.z
        );

        Vector3 centerOffset = new Vector3(manager.Width, manager.Height, manager.Depth) * 0.5f * manager.VoxelSize;
        Vector3 centeredGridPos = gridPos * manager.VoxelSize - centerOffset;

        float distanceFromCenter = centerOffset.magnitude;

        float heightPercent = Mathf.InverseLerp(manager.MinSR, manager.MaxSR, distanceFromCenter);

        return manager.PlanetGradient.Evaluate(heightPercent);
    }
    private Vector3 Interpolate(Vector3 p1, Vector3 p2, float v1, float v2) // linear interpolation, returns where the vertex should be along the edge
    {
        float t = (manager.IsoLevel - v1) / (v2 - v1);
        t = Mathf.Clamp01(t);
        return p1 + (p2 - p1) * t;
    }
    private void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(meshVerts);
        mesh.SetTriangles(meshTris, 0);
        mesh.RecalculateNormals();
        mesh.SetColors(meshColors);

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
}
