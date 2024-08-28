using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    [SerializeField] int chunkSize;
    [SerializeField] Vector3Int mapSize;
    [SerializeField] VoxelMeshBuilder chunkPrefab;
    [SerializeField] Voxel airVoxel;

    NoiseGenerator noiseGenerator;
    Voxel[,,] voxelData;
    Dictionary<Vector3Int, VoxelMeshBuilder> builtChunks;
    int realChunkSize;
    private void Awake()
    {
        noiseGenerator = GetComponent<NoiseGenerator>();
    }

    private void Start()
    {
        GenerateMap();
        BuildMap();
    }

    public void FillVoxels(Vector3Int[] voxelPositions, Voxel voxel)
    {
        List<Vector3Int> chunksToRegen = new List<Vector3Int>();
        foreach (Vector3Int voxelPos in voxelPositions)
        {
            if (voxelPos.x >= 1 && voxelPos.x < voxelData.GetLength(0) - 1)
            {
                if (voxelPos.y >= 1 && voxelPos.y < voxelData.GetLength(1) - 1)
                {
                    if (voxelPos.z >= 1 && voxelPos.z < voxelData.GetLength(2) - 1)
                    {
                        voxelData[voxelPos.x, voxelPos.y, voxelPos.z] = voxel;
                        Vector3Int chunkPos = GetChunkPosOfVoxel(voxelPos);
                        Vector3Int[] neighbourChunks = new Vector3Int[]
                        {
                            new Vector3Int(0, 0, 0),
                            new Vector3Int(1, 0, 0),
                            new Vector3Int(0, 1, 0),
                            new Vector3Int(0, 0, 1),
                            new Vector3Int(-1, 0, 0),
                            new Vector3Int(0, -1, 0),
                            new Vector3Int(0, 0, -1),
                        };
                        foreach (Vector3Int offset in neighbourChunks )
                        {
                            if (!chunksToRegen.Contains(chunkPos + offset)) chunksToRegen.Add(chunkPos + offset);
                        }
                    }
                }
            }
        }
        foreach (Vector3Int chunkPos in chunksToRegen)
        {
            if (builtChunks.TryGetValue(chunkPos * realChunkSize, out VoxelMeshBuilder chunk))
            {
                chunk.BuildChunk(getChunkDataFromChunkPos(chunkPos * realChunkSize));
            }
        }
    }

    public void SetVoxel(Vector3Int voxelPos, Voxel voxel)
    {
        if (voxelPos.x >= 1 && voxelPos.x < voxelData.GetLength(0) - 1)
        {
            if (voxelPos.y >= 1 && voxelPos.y < voxelData.GetLength(1) - 1)
            {
                if (voxelPos.z >= 1 && voxelPos.z < voxelData.GetLength(2) - 1)
                {
                    voxelData[voxelPos.x, voxelPos.y, voxelPos.z] = voxel;
                    Vector3Int[] chunksToUpdate = getChunksToUpdate(voxelPos);

                    foreach (Vector3Int chunkPos in chunksToUpdate)
                    {
                        if (builtChunks.TryGetValue(chunkPos * realChunkSize, out VoxelMeshBuilder chunk))
                        {
                            chunk.BuildChunk(getChunkDataFromChunkPos(chunkPos * realChunkSize));
                        }
                    }
                }
            }
        }
    }

    Vector3Int[] getChunksToUpdate(Vector3Int voxelPos)
    {
        List<Vector3Int> chunksToUpdate = new List<Vector3Int>();
        Vector3Int chunkPos = GetChunkPosOfVoxel(voxelPos);
        chunksToUpdate.Add(chunkPos);
        Vector3Int chunkRelative = new Vector3Int(voxelPos.x - (chunkPos.x * realChunkSize), voxelPos.y - (chunkPos.y * realChunkSize), voxelPos.z - (chunkPos.z * realChunkSize));
        Debug.Log(chunkRelative);
        if (chunkRelative.x >= realChunkSize) chunksToUpdate.Add(new Vector3Int(1, 0, 0) + chunkPos);
        if (chunkRelative.x <= 1) chunksToUpdate.Add(new Vector3Int(-1, 0, 0) + chunkPos);
        if (chunkRelative.y >= realChunkSize) chunksToUpdate.Add(new Vector3Int(0, 1, 0) + chunkPos);
        if (chunkRelative.y <= 1) chunksToUpdate.Add(new Vector3Int(0, -1, 0) + chunkPos);
        if (chunkRelative.z >= realChunkSize) chunksToUpdate.Add(new Vector3Int(0, 0, 1) + chunkPos);
        if (chunkRelative.z <= 1) chunksToUpdate.Add(new Vector3Int(0, 0, -1) + chunkPos);

        return chunksToUpdate.ToArray();
    }

    public Voxel GetVoxel(Vector3Int voxelPos)
    {
        if (voxelPos.x >= 1 && voxelPos.x < voxelData.GetLength(0) - 1)
            if (voxelPos.y >= 1 && voxelPos.y < voxelData.GetLength(1) - 1)
                if (voxelPos.z >= 1 && voxelPos.z < voxelData.GetLength(2) - 1)
                {
                    return voxelData[voxelPos.x, voxelPos.y, voxelPos.z];
                }
        return null;
    }

    void GenerateMap()
    {
        realChunkSize = chunkSize - 2;
        voxelData = new Voxel[mapSize.x * realChunkSize + 2, mapSize.y * realChunkSize + 2, mapSize.z * realChunkSize + 2];
        for (int x = 0; x < mapSize.x * realChunkSize + 2; x++)
        {
            for (int y = 0; y < mapSize.y * realChunkSize + 2; y++)
            {
                for (int z = 0; z < mapSize.z * realChunkSize + 2; z++)
                {
                    voxelData[x, y, z] = noiseGenerator.GetVoxelAtPos(new Vector3Int(x, y, z));
                    if (x == 0 || x == mapSize.x * realChunkSize + 1) voxelData[x, y, z] = airVoxel;
                    if (y == 0 || y == mapSize.y * realChunkSize + 1) voxelData[x, y, z] = airVoxel;
                    if (z == 0 || z == mapSize.z * realChunkSize + 1) voxelData[x, y, z] = airVoxel;
                }
            }
        }

    }

    void BuildMap()
    {
        builtChunks = new Dictionary<Vector3Int, VoxelMeshBuilder>();
        for (int chunkX = 0; chunkX < mapSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < mapSize.y; chunkY++)
            {
                for (int chunkZ = 0; chunkZ < mapSize.z; chunkZ++)
                {
                    BuildChunk(chunkX, chunkY, chunkZ);
                }
            }
        }
    }

    Voxel[,,] getChunkDataFromChunkPos(Vector3Int chunkPos)
    {
        Voxel[,,] chunkData = new Voxel[chunkSize, chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    Vector3Int voxelWorldPos = new Vector3Int(chunkPos.x + x, chunkPos.y + y, chunkPos.z + z);
                    chunkData[x, y, z] = voxelData[voxelWorldPos.x, voxelWorldPos.y, voxelWorldPos.z];
                }
            }
        }
        return chunkData;
    }

    void BuildChunk(int chunkX, int chunkY, int chunkZ)
    {
        Vector3Int chunkPos = new Vector3Int(chunkX * realChunkSize, chunkY * realChunkSize, chunkZ * realChunkSize);
        Voxel[,,] chunkData = getChunkDataFromChunkPos(chunkPos);
        VoxelMeshBuilder chunkI = Instantiate(chunkPrefab);
        chunkI.transform.position = chunkPos;
        chunkI.BuildChunk(chunkData);
        builtChunks.Add(chunkPos, chunkI);
    }

    Vector3Int GetChunkPosOfVoxel(Vector3Int voxelPos)
    {
        int x = Mathf.FloorToInt((float)(voxelPos.x - 1) / realChunkSize);
        int y = Mathf.FloorToInt((float)(voxelPos.y - 1) / realChunkSize);
        int z = Mathf.FloorToInt((float)(voxelPos.z - 1) / realChunkSize);
        return new Vector3Int(x, y, z);
    }
}
