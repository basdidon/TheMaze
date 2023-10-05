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

public struct Portal
{
    public Section FromSection { get; }
    public Floor FromFloor => FromSection.Floor;
    public Vector2Int FromLocalPos { get; }

    public Section ToSection { get; }
    public Floor ToFloor => ToSection.Floor;
    public Vector2Int ToLocalPos { get; }

    public Portal(Section fromSection, Section toSection, Vector2Int fromLocalPos, Vector2Int toLocalPos)
    {
        FromSection = fromSection;
        ToSection = toSection;
        FromLocalPos = fromLocalPos;
        ToLocalPos = toLocalPos;
    }
}