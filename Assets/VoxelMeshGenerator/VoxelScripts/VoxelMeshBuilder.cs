using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshBuilder : MonoBehaviour
{
    [SerializeField] int atlasSize;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void BuildChunk(Voxel[,,] voxelData)
    {
        Mesh mesh = GenerateMesh(voxelData);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    Mesh GenerateMesh(Voxel[,,] voxelData)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        List<CombineInstance> combineMeshes = new List<CombineInstance>();

        for (int x = 1; x < voxelData.GetLength(0) - 1; x++)
            for (int y = 1; y < voxelData.GetLength(1) - 1; y++)
                for (int z = 1; z < voxelData.GetLength(2) - 1; z++)
                {
                    ComplexVoxel cv = voxelData[x, y, z] as ComplexVoxel;
                    if (cv != null)
                    {
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = cv.mesh;
                        Matrix4x4 transformMatrix = new Matrix4x4();
                        transformMatrix[0, 3] = x;
                        transformMatrix[1, 3] = y;
                        transformMatrix[2, 3] = z;
                        ci.transform = transformMatrix;
                        combineMeshes.Add(ci);
                    }
                    else
                    {
                        Debug.Log("Yo");
                        Vector3[] VertPos = new Vector3[8]{
                        new Vector3(-1, 1, -1), new Vector3(-1, 1, 1),
                        new Vector3(1, 1, 1), new Vector3(1, 1, -1),
                        new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                        new Vector3(1, -1, 1), new Vector3(1, -1, -1),
                        };

                        int[,] Faces = new int[6, 7]{
                        {0, 1, 2, 3, 0, 1, 0},     //top
                        {7, 6, 5, 4, 0, -1, 0},   //bottom
                        {2, 1, 5, 6, 0, 0, 1},     //right
                        {0, 3, 7, 4, 0, 0, -1},   //left
                        {3, 2, 6, 7, 1, 0, 0},    //front
                        {1, 0, 4, 5, -1, 0, 0}    //back
                        };

                        if (!voxelData[x, y, z].nonSolid)
                            for (int o = 0; o < 6; o++)
                            {
                                if (voxelData[x + Faces[o, 4], y + Faces[o, 5], z + Faces[o, 6]].nonSolid)
                                    AddQuad(o, vertices.Count);
                            }

                        void AddQuad(int facenum, int v)
                        {
                            // Add Mesh
                            for (int i = 0; i < 4; i++) vertices.Add(new Vector3(x, y, z) + VertPos[Faces[facenum, i]] / 2f);
                            triangles.AddRange(new List<int>() { v, v + 1, v + 2, v, v + 2, v + 3 });

                            // Add uvs
                            Vector2 bottomleft = voxelData[x, y, z].uvCoordinate / atlasSize;

                            uvs.AddRange(new List<Vector2>() { bottomleft + new Vector2(0, 1f) / atlasSize, bottomleft + new Vector2(1f, 1f) / atlasSize, bottomleft + new Vector2(1f, 0) / atlasSize, bottomleft });
                        }
                    }
                    
                }

        Mesh voxelMesh = new Mesh();
        voxelMesh.vertices = vertices.ToArray();
        voxelMesh.uv = uvs.ToArray();
        voxelMesh.triangles = triangles.ToArray();
        voxelMesh.RecalculateNormals();
        voxelMesh.RecalculateBounds();
        

        CombineInstance combineInstance = new CombineInstance();
        combineInstance.mesh = voxelMesh;
        Mesh mesh = new Mesh();
        combineMeshes.Add(combineInstance);
        mesh.CombineMeshes(combineMeshes.ToArray());

        return mesh;
    }
}
