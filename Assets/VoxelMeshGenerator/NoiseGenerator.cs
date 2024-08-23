using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class NoiseGenerator : MonoBehaviour
{
    [SerializeField] Voxel fullVoxel;
    [SerializeField] Voxel otherFullVoxel;
    [SerializeField] Voxel airVoxel;
    [SerializeField] float noiseScale;
    [SerializeField] float threashold;
    public Voxel GetVoxelAtPos(Vector3Int pos)
    {
        float noiseVal = noise.snoise(new float3((float)pos.x, (float)pos.y, (float)pos.z) / noiseScale);
        if (noiseVal < threashold)
        {
            if (UnityEngine.Random.Range(1, 3) == 1) return fullVoxel;
            else return otherFullVoxel;
        }
        return airVoxel;
    }
}
