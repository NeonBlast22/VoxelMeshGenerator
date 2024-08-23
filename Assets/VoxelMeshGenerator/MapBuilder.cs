using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    [SerializeField] int chunkSize;
    [SerializeField] Vector3Int mapSize;
    [SerializeField] VoxelMeshBuilder chunkPrefab;

    NoiseGenerator noiseGenerator;
    Voxel[,,] voxelData;
    Dictionary<Vector3Int, VoxelMeshBuilder> builtChunks;

    private void Awake()
    {
        noiseGenerator = GetComponent<NoiseGenerator>();
    }

    private void Start()
    {
        GenerateMap();
        BuildMap();
    }

    public void SetVoxel(Vector3Int voxelPos, Voxel voxel)
    {
        if (voxelPos.x >= 0 && voxelPos.x < voxelData.GetLength(0))
            if (voxelPos.y >= 0 && voxelPos.y < voxelData.GetLength(1))
                if (voxelPos.z >= 0 && voxelPos.z < voxelData.GetLength(2))
                {
                    voxelData[voxelPos.x, voxelPos.y, voxelPos.z] = voxel;
                    Vector3Int chunkPos = GetChunkPosOfVoxel(voxelPos);
                    if (builtChunks.TryGetValue(chunkPos, out VoxelMeshBuilder chunk))
                    {
                        chunk.BuildChunk(getChunkDataFromChunkPos(chunkPos));
                    }
                }
    }

    public Voxel GetVoxel(Vector3Int voxelPos)
    {
        if (voxelPos.x >= 0 && voxelPos.x < voxelData.GetLength(0))
            if (voxelPos.y >= 0 && voxelPos.y < voxelData.GetLength(1))
                if (voxelPos.z >= 0 && voxelPos.z < voxelData.GetLength(2))
                {
                    return voxelData[voxelPos.x, voxelPos.y, voxelPos.z];
                }
        return null;
    }

    void GenerateMap()
    {
        voxelData = new Voxel[mapSize.x * chunkSize, mapSize.y * chunkSize, mapSize.z * chunkSize];
        for (int x = 0; x < mapSize.x * chunkSize; x++)
        {
            for (int y = 0; y < mapSize.y * chunkSize; y++)
            {
                for (int z = 0; z < mapSize.z * chunkSize; z++)
                {
                    voxelData[x, y, z] = noiseGenerator.GetVoxelAtPos(new Vector3Int(x, y, z));
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
        Vector3Int chunkPos = new Vector3Int(chunkX * (chunkSize - 2), chunkY * (chunkSize - 2), chunkZ * (chunkSize - 2));
        Voxel[,,] chunkData = getChunkDataFromChunkPos(chunkPos);
        VoxelMeshBuilder chunkI = Instantiate(chunkPrefab);
        chunkI.transform.position = chunkPos;
        chunkI.BuildChunk(chunkData);
        builtChunks.Add(chunkPos, chunkI);
    }

    Vector3Int GetChunkPosOfVoxel(Vector3Int voxelPos)
    {
        int x = Mathf.FloorToInt((float)voxelPos.x / chunkSize);
        int y = Mathf.FloorToInt((float)voxelPos.y / chunkSize);
        int z = Mathf.FloorToInt((float)voxelPos.z / chunkSize);
        return new Vector3Int(x, y, z);
    }
}
