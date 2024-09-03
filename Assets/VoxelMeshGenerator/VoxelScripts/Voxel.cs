using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Voxel", menuName = "ScriptableObjects/Voxel")]
public class Voxel : ScriptableObject
{
    public Vector2 uvCoordinate;
    public bool nonSolid;
}
