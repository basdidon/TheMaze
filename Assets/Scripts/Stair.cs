using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stair
{
    public Vector2Int LocalCellPos { get; }

    public Vector2Int TargetWorldPos => TargetFloor.LocalToWorldPos(LocalCellPos);
    public Section TargetSection => TargetFloor.GetSection(TargetWorldPos);
    public Floor TargetFloor { get; }

    public Stair(Vector2Int localPos, Floor floor)
    {
        LocalCellPos = localPos;
        TargetFloor = floor;
    }
}