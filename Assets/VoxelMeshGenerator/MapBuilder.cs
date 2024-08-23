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

    private void Awake()
    {
        noiseGenerator = GetComponent<NoiseGenerator>();
    }

    private void Start()
    {
        GenerateMap();
        BuildMap();
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
        for (int chunkX = 0; chunkX < mapSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < mapSize.y; chunkY++)
            {
                for (int chunkZ = 0; chunkZ < mapSize.z; chunkZ++)
                {
                    Vector3Int chunkPos = new Vector3Int(chunkX * (chunkSize - 2), chunkY * (chunkSize - 2), chunkZ * (chunkSize - 2));
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
                    BuildChunk(chunkPos, chunkData);
                }
            }
        }
    }

    void BuildChunk(Vector3Int chunkPos, Voxel[,,] chunkData)
    {
        VoxelMeshBuilder chunkI = Instantiate(chunkPrefab);
        chunkI.transform.position = chunkPos;
        chunkI.BuildChunk(chunkData);
    }

    Vector3Int GetChunkPosOfVoxel(Vector3Int voxelPos)
    {
        int x = Mathf.FloorToInt((float)voxelPos.x / chunkSize);
        int y = Mathf.FloorToInt((float)voxelPos.y / chunkSize);
        int z = Mathf.FloorToInt((float)voxelPos.z / chunkSize);
        return new Vector3Int(x, y, z);
    }
}
